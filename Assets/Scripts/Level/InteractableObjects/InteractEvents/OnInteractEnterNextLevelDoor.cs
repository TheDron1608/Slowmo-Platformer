using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterNextLevelDoor : AbstractLevelFinishDoor
{
    public bool NewLoop = false;

    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        SpawnManager.Instance.FinishGameplay(interactor?.GetComponent<AbstractCharacterComponent>(), SceneList.GAMEPLAY);

        if (NewLoop)
        {
            DifficultyManager.Instance.Loop++;
        }
    }
}
