using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;

[DefaultExecutionOrder(-1)]
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
    public EnemySpawnInfo MinionSpawnInfo;

    private CharacterComponentsManager _playerCharacter;
    private CharacterComponentsManager _boss;
    private List<CharacterComponentsManager> _minions = new();
    private bool _bossSpawned = false;
    private bool _bossTriggered = false;
    private bool _bossSkipped = false;
    private bool _wonGame = false;
    private Coroutine _bossDialogueCoroutine = null;
    private Holdable _lastBossKillHoldable = null;
    private bool _gibMinionsOnBossDeath = true;

    public bool BossIsTriggered()
    {
        return _bossTriggered;
    }

    public CharacterComponentsManager Boss
    {
        get => _boss;
        set => _boss = value;
    }

    public bool GibMinionsOnBossDeath
    {
        get => _gibMinionsOnBossDeath;
        set => _gibMinionsOnBossDeath = value;
    }

    private void Start()
    {
        Instance = this;

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.enabled = true;
            modificator.DisabledModificator = modificator.ModificatorType == AbstractModificator.ModificatorTypes.NEGATIVE;
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
        _lastBossKillHoldable = _playerCharacter.CharacterHolding.CurrentHoldObject ?? _playerCharacter.CharacterHolding.CurrentHolsteredHoldObject;

        PlayerCharacterInfo currentBoss = SessionManager.Instance.TotalCharacters.Find(e => e.name == SessionManager.Instance.CurrentSession.CurrentBossName);
        ZIndexLayer throneLayer = LayerManager.Instance.GetZLayerOfGameObject(Throne.gameObject);
        if (currentBoss != null)
        {
            _bossSpawned = true;

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
            _boss.CharacterHolding.HolsterNewHoldable(SessionManager.Instance.GetHoldableByUniqueCode(SessionManager.Instance.CurrentSession.CurrentBossWeapon));
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
                CharacterComponentsManager minion = MinionSpawnInfo.SpawnAt(minionSpawnPosition.position, throneLayer);
                _minions.Add(minion);
                minion.CharacterAIManager.SetAIDisabled(true);
                minion.CharacterVisual.FlippedH = _boss.transform.position.x > minion.transform.position.x;
            }

            List<AbstractModificator> mods = NumberMath.MergeLists(
                currentBoss.StartModificators,
                ModificatorsManager.Instance.ModificatorsPool.Where(e => SessionManager.Instance.CurrentSession.CurrentBossModificators.Contains(e.name)).ToList()
                );
            foreach (var bossStartMod in mods)
            {
                if (
                    (bossStartMod is IInvertableTeamModificator && bossStartMod.ModificatorType == AbstractModificator.ModificatorTypes.POSITIVE) ||
                    bossStartMod.ModificatorType == AbstractModificator.ModificatorTypes.NEUTRAL
                    )
                {
                    ModificatorsManager.Instance.AddModificator(bossStartMod, AbstractModificator.ModificatorStatuses.BOSS, true);
                }
            }

            _playerCharacter.CharacterAttacking.OnEffectApplied += PlayerCharacter_OnEffectApplied;
        }

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelGenerated();
            }
        }
    }

    private void PlayerCharacter_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (e.Receiver.TryGetComponent(out AbstractCharacterComponent character) && _minions.Contains(character.CharComponents))
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

        NextLevelDoor.enabled = true;
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
        if (!_bossSpawned)
        {
            Throne.enabled = true;
            if (Throne.GetCurrentSitter()?.CharComponents == _playerCharacter && !_wonGame)
            {
                Win();
                _wonGame = true;
            }
        }
        else if (!_bossTriggered || _bossDialogueCoroutine != null)
        {
            if (
                !_boss.IsDestroyed() &&
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
            if (_boss == null || _boss.IsDestroyed() || _boss.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
            {
                if (!Throne.enabled)
                {
                    NextLevelDoor.enabled = false;
                    Throne.enabled = true;
                    NavPointersScreenOverlay.Instance.UpdateNavTargets();
                }

                if (GibMinionsOnBossDeath)
                {
                    foreach (var minion in _minions)
                    {
                        if (minion.IsDestroyed() || minion.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()) continue;
                        minion.CharacterHealth.Gib(null);
                    }
                }
            }
            else
            {
                if (Throne.enabled)
                {
                    Throne.enabled = false;
                    NavPointersScreenOverlay.Instance.UpdateNavTargets();
                }
            }

            if (_boss != null && !_boss.IsDestroyed()) _boss.CharacterAIManager.SetAIDisabled(false);

            if (_playerCharacter != null && Throne.GetCurrentSitter()?.CharComponents == _playerCharacter && !_wonGame)
            {
                Win();
                _wonGame = true;
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

        BossQuote(DifficultyManager.Instance.Loop <= 1 ? BossQuotes : BossLoopQuotes);

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
    }

    private void Win()
    {
        GameOverManager.Instance.ForceFinishGame(GameOverUIManager.GameOverReasons.FINISHED_GAME);
        _playerCharacter.CharacterHolding.TryHolster(_playerCharacter.CharacterHolding.CurrentHoldObject);
        _playerCharacter.CharacterHolding.ForceDisarm();
        _playerCharacter.CharacterAIManager.SetAIDisabled(false);

        if (SessionManager.Instance?.CurrentSession != null)
        {
            SessionManager.Instance.CurrentSession.CurrentBossName = SessionManager.Instance.CurrentSelectedPlayer.name;
            SessionManager.Instance.CurrentSession.CurrentBossWeapon = _lastBossKillHoldable?.FindingUniqueCodeName;
            SessionManager.Instance.CurrentSession.CurrentBossModificators.Clear();
            foreach (var mod in ModificatorsManager.Instance.CurrentModificators)
            {
                if (
                    mod.Status != AbstractModificator.ModificatorStatuses.BOSS &&
                    (
                        (mod is IInvertableTeamModificator && mod.ModificatorType != AbstractModificator.ModificatorTypes.NEGATIVE) ||
                        mod.ModificatorType == AbstractModificator.ModificatorTypes.NEUTRAL
                    )
                    )
                {
                    SessionManager.Instance.CurrentSession.CurrentBossModificators.Add(mod.OriginalModificator.name);
                }
            }

            SessionManager.Instance.SaveCurrentSession();
        }
    }

    public void EnableDisabledModsBack()
    {
        if (ModificatorsManager.Instance != null)
        {
            for (int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
            {
                if (ModificatorsManager.Instance.CurrentModificators[i].Status == AbstractModificator.ModificatorStatuses.BOSS)
                {
                    ModificatorsManager.Instance.RemoveModificatorAt(i);
                    i--;
                }
                else
                {
                    ModificatorsManager.Instance.CurrentModificators[i].DisabledModificator = false;
                }
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;

        if (_playerCharacter != null && !_playerCharacter.IsDestroyed()) _playerCharacter.CharacterAttacking.OnEffectApplied -= PlayerCharacter_OnEffectApplied;
    }
}
