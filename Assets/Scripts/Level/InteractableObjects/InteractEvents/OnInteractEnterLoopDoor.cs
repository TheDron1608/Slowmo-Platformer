using UnityEngine;

public class OnInteractEnterLoopDoor : AbstractLevelFinishDoor
{
    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        DifficultyManager.Instance.Loop++;

        SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.LOOP);
    }
}
