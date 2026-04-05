public class SellCursesCard : AbstractSingleCardItem
{
    public override void Pick()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out BlessPickManager blessContainer))
        {
            blessContainer.SellCurses();
        }
    }
}