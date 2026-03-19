using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterCurse : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        UIManager.Instance.LoadSceneWithEffect(SceneList.CURSE);
    }
}
