using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterNextLevelDoor : AnimatedInteractable
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.OnLevelFinished();
        }
        TeamManager.Instance.OnLevelFinished();

        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }
}
