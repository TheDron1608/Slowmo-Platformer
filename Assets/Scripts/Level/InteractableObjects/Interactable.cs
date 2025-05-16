using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterVisual;

public abstract class Interactable : SelectableObject
{
   
    const string INTERACTABLE_TAG_NAME = "Interactable";

    private GameObject _currentInteractor = null;

    public GameObject CurrentInteractor
    {
        get => _currentInteractor;
        private set => _currentInteractor = value;
    }

    [Header("Interactalbe")]
    public CharacterPartBusyStates AnimationOnStartInteract = CharacterPartBusyStates.NONE;
    public CharacterPartBusyStates AnimationOnFinishInteract = CharacterPartBusyStates.NONE;
    public List<AbstractEffect> EffectsOnStartInteract = new();
    public List<AbstractEffect> EffectsWhileInteracting = new();
    public List<AbstractEffect> EffectsOnFinishInteract = new();

    public bool GetIsOccured()
    {
        return _currentInteractor != null;
    }

    public bool ForceInteract(GameObject interactor)
    {
        StopInteract();
        return TryInteract(interactor);
    }

    public bool TryInteract(GameObject interactor)
    {
        OnPreInteract(interactor);

        if (!StartInteractCondition(interactor)) return false;

        OnStartInteact(interactor);

        if (interactor.TryGetComponent(out CharacterVisual characterVisual))
        {
            _currentInteractor = interactor;
            if (AnimationOnStartInteract != CharacterVisual.CharacterPartBusyStates.NONE)
            {
                characterVisual.CurrentBusyAnimation = AnimationOnStartInteract;
            }
            characterVisual.OnBusyStateChanged += CharacterVisual_OnFirstBusyStateChanged;
        }
        return true;
    }

    private void CharacterVisual_OnFirstBusyStateChanged(object sender, OnBusyStateChangedEventArgs e)
    {
        if (_currentInteractor == null) return;

        OnFinishInteract(_currentInteractor);

        CharacterVisual charVisual;
        if (!_currentInteractor.TryGetComponent(out charVisual)) return;


        charVisual.OnBusyStateChanged -= CharacterVisual_OnFirstBusyStateChanged;

        if (AnimationOnFinishInteract != CharacterVisual.CharacterPartBusyStates.NONE)
        {
            charVisual.OnBusyStateChanged += CharacterVisual_OnSecondBusyStateChanged;

            if (AnimationOnFinishInteract != CharacterVisual.CharacterPartBusyStates.NONE)
            {
                charVisual.CurrentBusyAnimation = AnimationOnFinishInteract;
            }
        }
        else
        {
            _currentInteractor = null;
        }
    }

    private void CharacterVisual_OnSecondBusyStateChanged(object sender, OnBusyStateChangedEventArgs e)
    {
        if (_currentInteractor == null) return;

        OnFinishInteractAnimationFinished(_currentInteractor);

        CharacterVisual charVisual = _currentInteractor.GetComponent<CharacterVisual>();
        if (charVisual == null) return;

        charVisual.OnBusyStateChanged -= CharacterVisual_OnSecondBusyStateChanged;

        _currentInteractor = null;
    }

    public void StopInteract()
    {
        OnStopInteract(_currentInteractor);

        CharacterVisual charVisual = _currentInteractor.GetComponent<CharacterVisual>();
        if (charVisual == null) return;

        charVisual.OnBusyStateChanged -= CharacterVisual_OnFirstBusyStateChanged;
        charVisual.OnBusyStateChanged -= CharacterVisual_OnSecondBusyStateChanged;

        charVisual.BreakBusyAnimation();

        _currentInteractor = null;
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
            charComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsWhileInteracting, this);
        }
    }
    /// <summary>
    /// called when StartAnimation is finished, not called if StartAnimation is not sat
    /// </summary>
    protected virtual void OnFinishInteract(GameObject interactor)
    {
        if (interactor.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterEffectsReceiver.RemoveEffect(EffectsWhileInteracting);
            charComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnFinishInteract, this);
        }
    }
    /// <summary>
    /// Called when FinishAnimation is finished, not called if FinishAnimation is not sat
    /// </summary>
    protected virtual void OnFinishInteractAnimationFinished(GameObject interactor)
    {

    }

    /// <summary>
    /// called before OnStartInteract
    /// if returns false calls OnStopInteract
    /// </summary>
    protected virtual bool StartInteractCondition(GameObject interactor)
    {
        return true;
    }

    /// <summary>
    /// called when interact condinting returns false
    /// </summary>
    protected virtual void OnStopInteract(GameObject interactor)
    {

    }
}
