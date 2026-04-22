using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DifficultyCurseChoiseUI : AbstractModificatorCardsManager
{
    const float FINISH_TRADE_TEMP_SLOWMO = 1.5f;

    public List<AbstractEffect> CharacterEffectsOnFinish = new();
    public List<AbstractEffect> EnemiesEffectOnFinish = new();
    public float EnemyEffectAffectDistance = 10f;

    private int _picksLeft = 1;
    private string _requestSceneChangeOnFinish = null;

    private void OnEnable()
    {
        _picksLeft = ModificatorsManager.Instance.DifficultyUpNegativeModificatorsPickAmount;
        TimeManager.Instance.Paused = true;
    }

    public void InitDebugCurseOptions()
    {
        ClearAllCards();

        InvokeModificatorChoiseStarted();

        foreach (AbstractModificator modificator in ModificatorDebugManager.Instance.DebugModificators)
        {

            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddModificator(modificator);
            newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.PERMANENT;
            AddCard(newCluster);
        }

        InvokeModificatorChoiseFinished();

        _picksLeft = int.MaxValue;
        if (Cards.Count == 0) FinishTrade();
    }

    public void InitCurseOptions(float cursePrice)
    {
        ClearAllCards();

        InvokeModificatorChoiseStarted();

        List<AbstractModificator> addedModificators = new();
        for (int i = 0; i < ModificatorsManager.Instance.MaxModificatorOptions; i++)
        {
            List<AbstractModificator> addModificators = DifficultyManager.GetRandomCurseModificators(cursePrice, addedModificators);

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

        InvokeModificatorChoiseFinished();

        if (Cards.Count == 0) FinishTrade();
    }

    public void RequestSceneChangeOnFinish(string sceneName)
    {
        _requestSceneChangeOnFinish = sceneName;
    }

    public override void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;

        if (_picksLeft <= 0)
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

        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                layer.SetEnvromentMaterialDependOnDifficulty(DifficultyManager.Instance.CurrentDifficulty.Value);
            }
        }

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

    private void InvokeModificatorChoiseFinished()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnModificatorChoiseFinished(this);
            }
        }
    }
}
