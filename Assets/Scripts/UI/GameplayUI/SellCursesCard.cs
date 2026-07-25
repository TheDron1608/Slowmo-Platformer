public class SellCursesCard : AbstractSingleCardItem
{
    public override void Pick()
    {
        base.Pick();

        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out BlessPickManager blessContainer))
        {
            blessContainer.SellCurses();
        }
        else if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out TutorialBlessPickManager tutorialBlessContainer))
        {
            tutorialBlessContainer.SellCurses();
        }
    }
}