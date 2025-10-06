using System;
using UnityEngine;

public class OnInteractEnterNextLevelDoor : Interactable
{
    const string ANIMATOR_OPEN_TRIGGER_NAME = "Open";

    public string SceneName;

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);
        GetComponent<Animator>().SetTrigger(ANIMATOR_OPEN_TRIGGER_NAME);
    }

    protected override void OnFinishInteractAnimationFinished(GameObject interactor)
    {
        base.OnFinishInteractAnimationFinished(interactor);
        UIManager.Instance.LoadSceneWithEffect(SceneName);
    }
}
