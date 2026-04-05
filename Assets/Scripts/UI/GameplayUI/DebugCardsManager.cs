using System.Collections.Generic;

public class DebugCardsManager : AbstractModificatorCardsManager
{
    public List<AbstractModificator> ModificatorsOnStart = new();
    public AbstractModificator.ModificatorStatuses CardsStatus;
    public CharacterComponentsManager StartCharacter;

    public override void FinishTrade()
    {
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    public override void SpendPicksLeft(int amount = 1)
    {

    }

    private void Start()
    {
        SpawnManager.Instance.PlayerCharacter = StartCharacter;

        foreach (var modificator in ModificatorsOnStart)
        {
            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            newCluster.AddStatusOnPick = CardsStatus;
            newCluster.AddModificator(modificator);
            AddCard(newCluster);
        }
    }
}