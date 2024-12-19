using System;
using UnityEngine;

public abstract class LockActionsInteractable : Interactable
{
    protected override void OnStartInteact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterActions charActions))
        {
            charActions.SetIsAbleToDoAnyActions(false);
        }
    }

    protected override void OnFinishInteractAnimationFinished(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterActions charActions))
        {
            charActions.SetIsAbleToDoAnyActions(true);
        }
    }

    protected override void OnStopInteract(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterActions charActions))
        {
            charActions.SetIsAbleToDoAnyActions(true);
        }
    }
}
