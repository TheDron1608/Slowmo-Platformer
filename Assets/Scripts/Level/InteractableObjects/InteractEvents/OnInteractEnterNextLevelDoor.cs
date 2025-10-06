using System;
using UnityEngine;

public class OnInteractEnterNextLevelDoor : AnimatedInteractable
{
    public string SceneName;

    protected override void OnFinishInteractAnimationFinished(GameObject interactor)
    {
        base.OnFinishInteractAnimationFinished(interactor);
        UIManager.Instance.LoadSceneWithEffect(SceneName);
    }
}
