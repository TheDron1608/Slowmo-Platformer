using UnityEngine;

public class OnInteractEnterNextLevelDoor : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.GAMEPLAY);
    }
}
