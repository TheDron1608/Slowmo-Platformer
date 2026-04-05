public class PickNothingCard : AbstractSingleCardItem
{
    public override void Pick()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.FinishTrade();
        }
    }
}