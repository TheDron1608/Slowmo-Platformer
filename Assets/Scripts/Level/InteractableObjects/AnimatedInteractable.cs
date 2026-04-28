using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimatedInteractable : Interactable
{
    [Header("AnimatedInteractalbe")]
    public CharacterVisual.CharacterPartBusyStates AnimationOnStartInteract = CharacterVisual.CharacterPartBusyStates.NONE;
    public CharacterVisual.CharacterPartBusyStates AnimationOnFinishInteract = CharacterVisual.CharacterPartBusyStates.NONE;

    public string SelfAnimatorOnStartIntreactTriggerName = "";
    public string SelfAnimatorOnFinishIntreactTriggerName = "";

    public List<AbstractEffect> EffectsWhileInteracting = new();
    public List<AbstractEffect> EffectsOnFinishInteract = new();

    public event EventHandler OnFinishedInteract;

    /// <summary>
    /// called when StartAnimation started
    /// </summary>
    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);

        if (interactor.TryGetComponent(out CharacterVisual characterVisual))
        {
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

    private void CharacterVisual_OnFirstBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        OnFinishInteract((sender as MonoBehaviour).gameObject);

        if (SelfAnimatorOnFinishIntreactTriggerName != "")
        {
            GetComponent<Animator>()?.SetTrigger(SelfAnimatorOnFinishIntreactTriggerName);
        }

        CharacterVisual charVisual;
        if (!(sender as MonoBehaviour).TryGetComponent(out charVisual)) return;

        charVisual.OnBusyStateChanged -= CharacterVisual_OnFirstBusyStateChanged;

        if (AnimationOnFinishInteract != CharacterVisual.CharacterPartBusyStates.NONE)
        {
            if (AnimationOnFinishInteract != CharacterVisual.CharacterPartBusyStates.NONE)
            {
                charVisual.CurrentBusyAnimation = AnimationOnFinishInteract;
            }
        }
    }

    /// <summary>
    /// called when StartAnimation is finished, not called if StartAnimation is not sat
    /// </summary>
    protected virtual void OnFinishInteract(GameObject interactor)
    {
        OnFinishedInteract?.Invoke(this, EventArgs.Empty);

        if (interactor.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterEffectsReceiver.RemoveEffect(EffectsWhileInteracting);
            charComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnFinishInteract, this);
        }
    }
}
