using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterShopDoor : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        if (ModificatorsManager.Instance.CanSellCurses)
        {
            UIManager.Instance.LoadSceneWithEffect(SceneList.SHOP);
        }
        else
        {
            UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
        }
    }
}
