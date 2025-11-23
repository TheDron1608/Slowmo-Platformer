using UnityEngine;

public class OnInteractEnterNextLevelDoor : AnimatedInteractable
{
    public string SceneName;

    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);
        UIManager.Instance.LoadSceneWithEffect(SceneName);
    }
}
