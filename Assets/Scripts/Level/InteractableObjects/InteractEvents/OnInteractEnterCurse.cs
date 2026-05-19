using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterCurse : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        if (ModificatorsManager.Instance.CanPickCurses)
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.CURSE);
        }
        else
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.GAMEPLAY);
        }
    }
}
