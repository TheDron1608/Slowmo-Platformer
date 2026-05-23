public class RerollCard : AbstractSingleCardItem
{
    public override void Pick()
    {
        base.Pick();

        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.TryReroll();
        }
    }
}