public class PickNothingCard : AbstractSingleCardItem
{
    public override void Pick()
    {
        base.Pick();

        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.FinishTrade(true);
        }
    }
}