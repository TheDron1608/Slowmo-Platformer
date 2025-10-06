using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterVisual;

public abstract class Interactable : SelectableObject
{
   
    const string INTERACTABLE_TAG_NAME = "Interactable";

    [Header("Interactalbe")]
    public List<AbstractEffect> EffectsOnStartInteract = new();

    public bool GetIsValidToInteract(GameObject interactor)
    {
        return StartInteractCondition(interactor);
    }

    public bool TryInteract(GameObject interactor)
    {
        OnPreInteract(interactor);

        if (!StartInteractCondition(interactor)) return false;

        OnStartInteact(interactor);

        return true;
    }

    /// <summary>
    /// called before interact condition check
    /// </summary>
    protected virtual void OnPreInteract(GameObject interactor)
    {

    }
    /// <summary>
    /// called when StartAnimation started
    /// </summary>
    protected virtual void OnStartInteact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnStartInteract, this);
        }
    }

    /// <summary>
    /// called before OnStartInteract
    /// if returns false calls OnStopInteract
    /// </summary>
    protected virtual bool StartInteractCondition(GameObject interactor)
    {
        return true;
    }
}
