using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CharacterVisual;

public abstract class AnimatedInteractable : Interactable
{

    private GameObject _currentInteractor = null;

    public GameObject CurrentInteractor
    {
        get => _currentInteractor;
        private set => _currentInteractor = value;
    }

    [Header("AnimatedInteractalbe")]
    public CharacterPartBusyStates AnimationOnStartInteract = CharacterPartBusyStates.NONE;
    public CharacterPartBusyStates AnimationOnFinishInteract = CharacterPartBusyStates.NONE;

    public string SelfAnimatorOnStartIntreactTriggerName = "";
    public string SelfAnimatorOnFinishIntreactTriggerName = "";

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

    /// <summary>
    /// called when StartAnimation started
    /// </summary>
    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);

        if (interactor.TryGetComponent(out CharacterVisual characterVisual))
        {
            _currentInteractor = interactor;
            if (AnimationOnStartInteract != CharacterVisual.CharacterPartBusyStates.NONE)
            {
                characterVisual.CurrentBusyAnimation = AnimationOnStartInteract;
            }
            characterVisual.OnBusyStateChanged += CharacterVisual_OnFirstBusyStateChanged;
        }

        if (SelfAnimatorOnStartIntreactTriggerName != "")
        {
            GetComponent<Animator>().SetTrigger(SelfAnimatorOnStartIntreactTriggerName);
        }
    }

    private void CharacterVisual_OnFirstBusyStateChanged(object sender, OnBusyStateChangedEventArgs e)
    {
        if (_currentInteractor == null) return;

        OnFinishInteract(_currentInteractor);

        if (SelfAnimatorOnFinishIntreactTriggerName != "")
        {
            GetComponent<Animator>().SetTrigger(SelfAnimatorOnFinishIntreactTriggerName);
        }

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
    /// called when interact condinting returns false
    /// </summary>
    protected virtual void OnStopInteract(GameObject interactor)
    {

    }
}
