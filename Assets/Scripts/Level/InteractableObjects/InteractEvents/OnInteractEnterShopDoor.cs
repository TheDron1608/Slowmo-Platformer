using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterShopDoor : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        if (ModificatorsManager.Instance.CanSellCurses)
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.SHOP);
        }
        else
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.GAMEPLAY);
        }
    }
}
