public class RerollCard : AbstractSingleCardItem
{
    public override void Pick()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.TryReroll();
        }
    }
}