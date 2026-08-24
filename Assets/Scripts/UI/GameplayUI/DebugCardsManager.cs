using System.Collections.Generic;

public class DebugCardsManager : AbstractModificatorCardsManager
{
    public AbstractModificator.ModificatorStatuses CardsStatus;
    public CharacterComponentsManager StartCharacter;

    public override string GetAnalyticsChoiseTypeName()
    {
        return null;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        SpawnManager.Instance.PlayerCharacter = StartCharacter;

        AddDebugCards(ModificatorDebugManager.Instance.DebugModificators);
    }

    public override void FinishTrade(bool pickNothing = false)
    {
        base.FinishTrade(pickNothing);

        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    public override void SpendPicksLeft(int amount = 1)
    {

    }

    public void AddDebugCards(List<AbstractModificator> debugModificators)
    {
        foreach (var modificator in debugModificators)
        {
            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddStatusOnPick = CardsStatus;
            newCluster.AddModificator(modificator);
            AddCard(newCluster);
        }
    }
}