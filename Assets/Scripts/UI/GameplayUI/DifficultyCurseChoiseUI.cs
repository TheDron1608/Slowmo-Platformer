using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DifficultyCurseChoiseUI : AbstractModificatorCardsManager
{
    const float MAX_CURSE_PRICE_REDUCTION = 0.667f;
    const float FINISH_TRADE_TEMP_SLOWMO = 1.5f;

    public List<AbstractEffect> CharacterEffectsOnFinish = new();
    public List<AbstractEffect> EnemiesEffectOnFinish = new();
    public float EnemyEffectAffectDistance = 10f;

    private int _picksLeft = 1;

    private void OnEnable()
    {
        _picksLeft = ModificatorsManager.Instance.DifficultyUpNegativeModificatorsPickAmount;
        TimeManager.Instance.Paused = true;
    }

    public void InitCurseOptions(float cursePrice)
    {
        ClearAllCards();

        float currentBlessMaxPrice = cursePrice;
        for (int i = 0; i < ModificatorsManager.Instance.MaxModificatorOptions; i++)
        {
            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddModificator(ModificatorsManager.Instance.PickRandomModificators(AbstractModificator.ModificatorTypes.NEGATIVE, currentBlessMaxPrice - 1));
            if (newCluster == null) break;

            newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.PERMANENT;
            AddCard(newCluster);

            /*currentBlessMaxPrice =
                newCluster.Cards
                .Where(e => e.ModificatorInstance.ModificatorType == AbstractModificator.ModificatorTypes.NEGATIVE)
                .OrderBy(e => e.ModificatorInstance.ModificatorPrice)
                .First()
                .ModificatorInstance.ModificatorPrice;*/

            if (currentBlessMaxPrice < cursePrice * MAX_CURSE_PRICE_REDUCTION) break;
        }
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

        ApplyFinishEffects();

        UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Hide();
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
}
