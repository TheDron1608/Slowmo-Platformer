using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class DifficultyCurseChoiseUI : AbstractModificatorCardsManager
{
    const float FINISH_TRADE_TEMP_SLOWMO = 1.5f;

    public List<AbstractEffect> CharacterEffectsOnFinish = new();
    public List<AbstractEffect> EnemiesEffectOnFinish = new();
    public float EnemyEffectAffectDistance = 10f;

    private int _picksLeft = 1;
    private string _requestSceneChangeOnFinish = null;
    private float _initCurseMinPrice = 0f;
    private float _initCurseMaxPrice = 0f;
    private int _initPickAmount = 0;
    private int _initOptionsAmount = 0;
    private bool _isDebug = false;

    protected override string GetAnalyticsChoiseTypeName()
    {
        return _isDebug ? null : "DifficultyNegativePick";
    }

    private void Awake()
    {
        RerollsLeft = ModificatorsManager.Instance.DifficultyCursePickRerolls;
    }

    private void OnEnable()
    {
        _picksLeft = ModificatorsManager.Instance.DifficultyUpNegativeModificatorsPickAmount;
        TimeManager.Instance.Paused = true;
    }

    public void InitDebugCurseOptions()
    {
        _isDebug = true;

        ClearAllCards();

        InvokeModificatorChoiseStarted();

        foreach (AbstractModificator modificator in ModificatorDebugManager.Instance.DebugModificators)
        {

            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddModificator(modificator);
            newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.PERMANENT;
            AddCard(newCluster);
        }

        _picksLeft = int.MaxValue;
        if (Cards.Count == 0) FinishTrade();
    }

    public void InitCurseOptions(float curseMinPrice, float curseMaxPrice, int pickAmount, int optionsAmount)
    {
        _isDebug = false;

        ClearAllCards();

        InvokeModificatorChoiseStarted();

        _initCurseMinPrice = curseMinPrice;   
        _initCurseMaxPrice = curseMaxPrice;
        _initPickAmount = pickAmount;
        _initOptionsAmount = optionsAmount;

        List<AbstractModificator> addedModificators = new();
        for (int i = 0; i < math.max(_initOptionsAmount, _initPickAmount); i++)
        {
            List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
                AbstractModificator.ModificatorTypes.NEGATIVE,
                curseMinPrice,
                curseMaxPrice,
                false,
                true,
                true,
                addedModificators,
                false,
                ModificatorsManager.Instance.DifficultyCursePickCounterMods
                );

            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddModificator(addModificators);
            newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.PERMANENT;

            if (newCluster == null || newCluster.Cards.Count == 0)
            {
                break;
            }
            else
            {
                addedModificators.AddRange(addModificators);
                AddCard(newCluster);
            }
        }

        _picksLeft = _initPickAmount;

        if (Cards.Count == 0 || _picksLeft == 0)
        {
            FinishTrade();
        }
        else if (RerollsLeft > 0)
        {
            AddCard(Instantiate(_rerollCardInstace));
        }
    }

    public override void ForceReroll()
    {
        base.ForceReroll();

        InvokeModificatorChoiseStarted();

        List<AbstractModificator> addedModificators = new();
        for (int i = 0; i < math.max(_initOptionsAmount, _initPickAmount); i++)
        {
            List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
                AbstractModificator.ModificatorTypes.NEGATIVE,
                _initCurseMinPrice,
                _initCurseMaxPrice,
                false,
                true,
                true,
                addedModificators,
                false,
                ModificatorsManager.Instance.DifficultyCursePickCounterMods
                );

            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddModificator(addModificators);
            newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.PERMANENT;

            if (newCluster == null || newCluster.Cards.Count == 0)
            {
                break;
            }
            else
            {
                addedModificators.AddRange(addModificators);
                AddCard(newCluster);
            }
        }

        _picksLeft = _initPickAmount;

        if (Cards.Count == 0 || _picksLeft == 0)
        {
            FinishTrade();
        }
        else if (RerollsLeft > 0)
        {
            AddCard(Instantiate(_rerollCardInstace));
        }
    }

    public void RequestSceneChangeOnFinish(string sceneName)
    {
        _requestSceneChangeOnFinish = sceneName;
    }

    public override void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;

        if (_picksLeft <= 0 || Cards.Count == 0)
        {
            while (Cards.Count > 0)
            {
                RemoveCard(Cards.First());
            }
            FinishTrade();
        }
    }

    public override void FinishTrade()
    {
        base.FinishTrade();

        TimeManager.Instance.TryTemporalSlowTime(FINISH_TRADE_TEMP_SLOWMO);
        TimeManager.Instance.Paused = false;

        DifficultyManager.Instance.UpdateDifficultyEnviromentMaterial();

        UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Hide();

        ApplyFinishEffects();

        if (_requestSceneChangeOnFinish != null)
        {
            UIManager.Instance.LoadSceneWithEffect(_requestSceneChangeOnFinish);
            _requestSceneChangeOnFinish = null;
        }
    }

    private void ApplyFinishEffects()
    {
        foreach (CharacterTeam playerCharacter in TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers())
        {
            playerCharacter.CharComponents.CharacterEffectsReceiver.ApplyEffect(CharacterEffectsOnFinish, null);

            foreach (Transform characterTransform in LayerManager.Instance.GetZLayerOfGameObject(playerCharacter.gameObject).CharactersContainer)
            {
                if (
                    Vector2.Distance(playerCharacter.CharComponents.Center.transform.position, characterTransform.position) < EnemyEffectAffectDistance &&
                    characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                    !playerCharacter.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam)
                    )
                {
                    character.CharComponents.CharacterEffectsReceiver.ApplyEffect(EnemiesEffectOnFinish, playerCharacter);
                }
            }
        }
    }

    private void InvokeModificatorChoiseStarted()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnModificatorChoiseStarted(this);
            }
        }
    }
}
