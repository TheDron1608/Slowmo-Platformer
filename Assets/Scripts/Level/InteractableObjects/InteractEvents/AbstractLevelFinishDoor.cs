using UnityEngine;
public class AbstractLevelFinishDoor : AnimatedInteractable
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        for(int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
        {
            ModificatorsManager.Instance.CurrentModificators[i].OnLevelFinished();
        }
    }
}
