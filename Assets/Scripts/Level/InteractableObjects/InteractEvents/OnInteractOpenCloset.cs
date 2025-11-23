using UnityEngine;

public class OnInteractOpenCloset : Interactable
{
    const string ANIMATOR_CLOSED_PROP_NAME = "Closed";

    private bool _closed = true;

    private Animator _animator;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found at " + gameObject.name);
    }

    public bool Closed
    {
        get => _closed;
        set
        {
            _closed = value;
            _animator.SetBool(ANIMATOR_CLOSED_PROP_NAME, _closed);

            GetComponent<BreakableObject>()?.ReleaseObjectsInside();
        }
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);
        Closed = false;
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return base.StartInteractCondition(interactor) && Closed;
    }
}
