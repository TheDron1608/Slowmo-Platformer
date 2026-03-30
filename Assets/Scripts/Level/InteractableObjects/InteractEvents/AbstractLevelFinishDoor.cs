using UnityEngine;
public class AbstractLevelFinishDoor : AnimatedInteractable
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.OnLevelFinished();
        }
    }
}
