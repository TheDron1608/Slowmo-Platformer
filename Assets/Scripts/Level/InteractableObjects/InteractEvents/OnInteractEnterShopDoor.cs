using UnityEngine;

public class OnInteractEnterShopDoor : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        if (ComboMoneyModificator.Instance != null)
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.MONEY_SHOP);
        }
        else if (ModificatorsManager.Instance.CanSellCurses)
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.SHOP);
        }
        else
        {
            SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.GAMEPLAY);
        }
    }
}
