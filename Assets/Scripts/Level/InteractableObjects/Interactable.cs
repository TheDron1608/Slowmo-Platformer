using System.Collections;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    const string INTERACTABLE_TAG_NAME = "Interactable";

    private GameObject _currentInteractor = null;

    public GameObject CurrentInteractor
    {
        get => _currentInteractor;
        private set => _currentInteractor = value;
    }

    public CharacterPart.CharacterParnBusyStates AnimationOnStartInteract = CharacterPart.CharacterParnBusyStates.NONE;
    public CharacterPart.CharacterParnBusyStates AnimationOnFinishInteract = CharacterPart.CharacterParnBusyStates.NONE;
    public bool CanBreakInteractionWhileInteracting = true;

    public void ForceInteract(GameObject interactor)
    {
        StopInteract();
        Interact(interactor);
    }

    public void Interact(GameObject interactor)
    {
        OnPreInteract(interactor);

        if (!InteractCondition(interactor)) return;

        OnStartInteact(interactor);

        if (interactor != null && interactor.TryGetComponent(out CharacterVisual characterVisual))
        {
            _currentInteractor = interactor;

            characterVisual.CurrentBusyAnimation = AnimationOnStartInteract;
            characterVisual.OnIsBusyChanged += CharacterVisual_OnIsBusyChanged;

            if (CanBreakInteractionWhileInteracting)
            {
                StartCoroutine(CheckHasBrokenInteractionWhileInteracting(interactor));
            }
        }
    }

    private void CharacterVisual_OnIsBusyChanged(object sender, bool e)
    {
        if (!e)
        {
            OnFinishInteract(_currentInteractor);

            CharacterVisual characterVisual = _currentInteractor.GetComponent<CharacterVisual>();
            characterVisual.OnIsBusyChanged -= CharacterVisual_OnIsBusyChanged;
            characterVisual.CurrentBusyAnimation = AnimationOnFinishInteract;

            _currentInteractor = null;
        }
    }

    public void StopInteract()
    {
        if (_currentInteractor == null) return;

        OnStopInteract(_currentInteractor);

        if (_currentInteractor.TryGetComponent(out CharacterVisual characterVisual)) {
            characterVisual.OnIsBusyChanged -= CharacterVisual_OnIsBusyChanged;
            characterVisual.CurrentBusyAnimation = CharacterPart.CharacterParnBusyStates.NONE;
        }

        _currentInteractor = null;
    }

    private IEnumerator CheckHasBrokenInteractionWhileInteracting(GameObject interactor)
    {
        while (InteractCondition(interactor) && _currentInteractor != null)
        {
            yield return new WaitForEndOfFrame();
        }
        StopInteract();
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
    /// called when StartAnimation finished, finish animation started
    /// </summary>
    protected virtual void OnMiddleInteract(GameObject interactor)
    {

    }
    /// <summary>
    /// called when finish animation finished
    /// </summary>
    protected virtual void OnFinishInteract(GameObject interactor)
    {

    }
    /// <summary>
    /// called before OnStartInteract and every frame while is interacting
    /// if returns false calls OnStopInteract
    /// </summary>
    protected virtual bool InteractCondition(GameObject interactor)
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
