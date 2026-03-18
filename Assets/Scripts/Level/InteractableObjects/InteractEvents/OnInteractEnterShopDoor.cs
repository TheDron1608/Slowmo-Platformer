using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterShopDoor : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        UIManager.Instance.LoadSceneWithEffect(SceneList.SHOP);
    }
}
