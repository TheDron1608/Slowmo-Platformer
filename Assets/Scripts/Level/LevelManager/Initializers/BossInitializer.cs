using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;

public class BossInitializer : MonoBehaviour
{
    public static BossInitializer Instance;

    public float BossTriggerDistance = 15f;
    public float BossDialogueDelay = 1f;
    public CharacterAIManager BossAI;
    public List<Transform> BossMinionsSpawnPositions = new();
    public OnInteractEnterNextLevelDoor NextLevelDoor;
    public Transform PlayerSpawnPosition;
    public ZIndexLayer PlayerSpawnLayer;
    public OnInteractSit Throne;
    public FightBossCard FightBossCardInstance;
    public SkipBossCard SkipBossCardInstance;
    public List<LocalizedString> BossQuotes;
    public List<LocalizedString> BossWinQuotes;
    public List<LocalizedString> BossLoopQuotes;

    private CharacterComponentsManager _playerCharacter;
    private CharacterComponentsManager _boss;
    private List<CharacterComponentsManager> _minions = new();
    private bool _bossTriggered = false;
    private bool _bossSkipped = false;
    private Coroutine _bossDialogueCoroutine = null;

    private void Start()
    {
        Instance = this;

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            //re-enable modificators with errors
            modificator.enabled = true;
        }

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelPreGenerated();
            }
        }

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Show();
        UIManager.Instance.DifficultyScreenOverlay.Show();
        UIManager.Instance.NavPointersScreenOverlay.Show();

        //spawn player failed, restart level
        _playerCharacter = SpawnManager.Instance.SpawnPlayerCharacterAt(PlayerSpawnPosition.position, PlayerSpawnLayer);

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelGenerated();
            }
        }

        NextLevelDoor.enabled = false;

        PlayerCharacterInfo currentBoss = SessionManager.Instance.TotalCharacters.Find(e => e.name == SessionManager.Instance.CurrentSession.CurrentBossName);
        ZIndexLayer throneLayer = LayerManager.Instance.GetZLayerOfGameObject(Throne.gameObject);
        if (currentBoss != null)
        {
            _boss = throneLayer.TrySpawnObject(
                currentBoss.PlayerCharacter.gameObject,
                Throne.transform.position,
                null,
                null
                ).First().GetComponent<AbstractCharacterComponent>().CharComponents;

            _boss.CharacterTeam.Team = TeamManager.Teams.DEFAULT_ENEMY;
            Destroy(_boss.CharacterAIManager.gameObject);
            _boss.CharacterAIManager = Instantiate(BossAI, _boss.transform);
            _boss.CharacterAIManager.SetAIDisabled(true);
            _boss.CharacterInteract.TryInteract(Throne);
            _boss.CharacterHolding.HolsterNewHoldable(currentBoss.StartHoldable);
            if (GameObjectUtility.TryGetComponentInSelfOrChild(_boss.gameObject, out CharacterUITrack uiTrack))
            {
                uiTrack.TrackHealth = false;
                uiTrack.TrackIfDead = false;
                uiTrack.TrackIsDying = false;
                uiTrack.TrackHoldable = false;
                uiTrack.TrackCamera = false;
                uiTrack.RefreshAllTracks();
            }

            foreach (Transform minionSpawnPosition in BossMinionsSpawnPositions)
            {
                CharacterComponentsManager minion = NumberMath.PickRandomItem(SpawnManager.Instance.EnemyPool).SpawnAt(minionSpawnPosition.position, throneLayer);
                _minions.Add(minion);
                minion.CharacterAIManager.SetAIDisabled(true);
                minion.CharacterVisual.FlippedH = _boss.transform.position.x > minion.transform.position.x;

                minion.CharacterHealth.OnHitByProjectile += Minion_OnHitByProjectile;
            }
        }
    }

    private void Minion_OnHitByProjectile(object sender, AbstractProjectile e)
    {
        if (e.Owner.CharComponents == _playerCharacter)
        {
            TriggerBoss();
        }
    }

    public void Fight()
    {
        TriggerBoss();

        if (Camera.main.TryGetComponent(out CameraTrack ct))
        {
            ct.TrackTargets.Clear();
            ct.TrackTargets.Add(_playerCharacter.transform);
        }
        _playerCharacter.CharacterAIManager.SetAIDisabled(false);
    }

    public void SkipFight()
    {
        _bossSkipped = true;

        NextLevelDoor.enabled = true;
        NavPointersScreenOverlay.Instance?.UpdateNavTargets();

        if (Camera.main.TryGetComponent(out CameraTrack ct))
        {
            ct.TrackTargets.Clear();
            ct.TrackTargets.Add(_playerCharacter.transform);
        }
        _playerCharacter.CharacterAIManager.SetAIDisabled(false);
    }

    public void BossWinQuote()
    {
        BossQuote(BossWinQuotes);
    }

    private void FixedUpdate()
    {
        if (!_bossTriggered || _bossDialogueCoroutine != null)
        {
            if (
                Vector2.Distance(_playerCharacter.transform.position, _boss.transform.position) < BossTriggerDistance && 
                _bossDialogueCoroutine == null && 
                !_bossSkipped
                )
            {
                _bossDialogueCoroutine = StartCoroutine(BossDialogue());
            }
        }
        else
        {
            bool targetEnable = _boss.IsDestroyed() || _boss.CharacterHealth.Died;
            foreach (var minion in _minions) targetEnable &= minion.IsDestroyed() || minion.CharacterHealth.Died;
            if (targetEnable != Throne.enabled)
            {
                Throne.enabled = targetEnable;
                NavPointersScreenOverlay.Instance.UpdateNavTargets();
            }
            _boss.CharacterAIManager.SetAIDisabled(false);

            if (Throne.GetCurrentSitter()?.CharComponents == _playerCharacter)
            {
                GameOverManager.Instance.ForceFinishGame(GameOverUIManager.GameOverReasons.FINISHED_GAME);
                _playerCharacter.CharacterHolding.TryHolster(_playerCharacter.CharacterHolding.CurrentHoldObject);
                _playerCharacter.CharacterHolding.ForceDisarm();
                _playerCharacter.CharacterAIManager.SetAIDisabled(false);
            }
        }
    }

    private IEnumerator BossDialogue()
    {
        _playerCharacter.CharacterAIManager.SetAIDisabled(true);
        _playerCharacter.CharacterMoving.ForceMove(0f);
        if (Camera.main.TryGetComponent(out CameraTrack ct))
        {
            ct.TrackTargets.Clear();
            ct.TrackTargets.Add(_boss.transform);
        }

        yield return new WaitForSeconds(BossDialogueDelay);

        BossQuote(DifficultyManager.Instance.Loops <= 1 ? BossQuotes : BossLoopQuotes);

        while (_boss.CharacterVisual.GetHasPopup())
        {
            yield return new WaitForEndOfFrame();
        }

        UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Show(new() { FightBossCardInstance, SkipBossCardInstance }, 1, false, false);

        _bossDialogueCoroutine = null;
    }

    private void BossQuote(List<LocalizedString> quotes)
    {
        if (!_boss.IsDestroyed() && !_boss.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
        {
            if (_boss.CharacterVisual.GetSpeakSoundPlayer().DefaultSound != null)
            {
                _boss.CharacterVisual.PopupText(NumberMath.PickRandomItem(quotes).GetLocalizedString());
            }
            else
            {
                _boss.CharacterVisual.PopupText(". . .");
            }
        }
    }

    private void TriggerBoss()
    {
        if (_bossTriggered) return;

        _bossTriggered = true;
        foreach (var minion in _minions)
        {
            if (!minion.IsDestroyed()) minion.CharacterAIManager.SetAIDisabled(false);
        }

        if (GameObjectUtility.TryGetComponentInSelfOrChild(_boss.gameObject, out CharacterUITrack uiTrack))
        {
            uiTrack.TrackHealth = true;
            uiTrack.RefreshAllTracks();
        }

        NextLevelDoor.enabled = false;
    }

    private void OnDestroy()
    {
        Instance = null;

        foreach (CharacterComponentsManager minion in _minions)
        {
            if (minion.IsDestroyed()) continue;
            minion.CharacterHealth.OnHitByProjectile -= Minion_OnHitByProjectile;
        }
    }
}
