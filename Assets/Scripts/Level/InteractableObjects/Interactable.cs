using System.Collections;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    const string INTERACTABLE_TAG_NAME = "Interactable";

    private GameObject _currentInteractor = null;
    private bool _isDoingInteraction = false;

    public GameObject CurrentInteractor
    {
        get => _currentInteractor;
        private set => _currentInteractor = value;
    }

    public CharacterPart.CharacterPartBusyStates AnimationOnStartInteract = CharacterPart.CharacterPartBusyStates.NONE;
    public CharacterPart.CharacterPartBusyStates AnimationOnFinishInteract = CharacterPart.CharacterPartBusyStates.NONE;

    public bool GetIsOccured()
    {
        return _currentInteractor != null;
    }

    public void ForceInteract(GameObject interactor)
    {
        StopInteract();
        Interact(interactor);
    }

    public void Interact(GameObject interactor)
    {
        OnPreInteract(interactor);

        if (!StartInteractCondition(interactor)) return;

        OnStartInteact(interactor);

        if (interactor.TryGetComponent(out CharacterVisual characterVisual))
        {
            _currentInteractor = interactor;
            characterVisual.CurrentBusyAnimation = AnimationOnStartInteract;
            characterVisual.OnBusyAnimationFinished += CharacterVisual_OnFirstBusyAnimationFinished;
        }
    }

    private void CharacterVisual_OnFirstBusyAnimationFinished(object sender, System.EventArgs e)
    {
        OnFinishInteract(_currentInteractor);

        CharacterVisual charVisual = _currentInteractor.GetComponent<CharacterVisual>();
        if (charVisual == null) return;


        charVisual.OnBusyAnimationFinished -= CharacterVisual_OnFirstBusyAnimationFinished;

        if (AnimationOnFinishInteract != CharacterPart.CharacterPartBusyStates.NONE)
        {
            charVisual.OnBusyAnimationFinished += CharacterVisual_OnSecondBusyAnimationFinished;

            charVisual.CurrentBusyAnimation = AnimationOnFinishInteract;
        }
        else
        {
            _currentInteractor = null;
        }
    }

    private void CharacterVisual_OnSecondBusyAnimationFinished(object sender, System.EventArgs e)
    {
        OnFinishInteractAnimationFinished(_currentInteractor);

        CharacterVisual charVisual = _currentInteractor.GetComponent<CharacterVisual>();
        if (charVisual == null) return;

        charVisual.OnBusyAnimationFinished -= CharacterVisual_OnSecondBusyAnimationFinished;

        _currentInteractor = null;
    }

    public void StopInteract()
    {
        OnStopInteract(_currentInteractor);

        CharacterVisual charVisual = _currentInteractor.GetComponent<CharacterVisual>();
        if (charVisual == null) return;

        charVisual.OnBusyAnimationFinished -= CharacterVisual_OnFirstBusyAnimationFinished;
        charVisual.OnBusyAnimationFinished -= CharacterVisual_OnSecondBusyAnimationFinished;

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

    }
    /// <summary>
    /// called when StartAnimation is finished, not called if StartAnimation is not sat
    /// </summary>
    protected virtual void OnFinishInteract(GameObject interactor)
    {

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
