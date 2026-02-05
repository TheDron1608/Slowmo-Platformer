using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class Shield : DamagableObject
{
    const string ANIMATOR_RAISED_PROP_NAME = "Raised";

    private Animator _animatorComponent;
    private Holdable _holdableComponent;
    private bool _raised = false;
    private bool _isRaisingUp = false;
    private bool _isRaisingDown = false;

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _animatorComponent)) throw new UnityException("Animator component not found at " + gameObject.name);
        if (!TryGetComponent(out _holdableComponent)) throw new UnityException("Holdable component not found at " + gameObject.name);    
    }

    public bool Raised
    {
        get => _raised;
        set
        {
            _raised = value;
            _holdableComponent.HitableWhenIsHolded = value;
        }
    }

    public bool GetIsRaising()
    {
        return _isRaisingDown || _isRaisingUp;
    }

    public bool TryRaiseUp()
    {
        _animatorComponent.SetBool(ANIMATOR_RAISED_PROP_NAME, true);
        _isRaisingUp = true;
        _isRaisingDown = false;

        return true;
    }

    public bool TryRaiseDown()
    {
        _animatorComponent.SetBool(ANIMATOR_RAISED_PROP_NAME, false);
        _isRaisingDown = true;
        _isRaisingUp = false;

        return true;
    }

    public void Animator_OnRaisedChanged(bool value)
    {
        _isRaisingUp = false;
        _isRaisingDown = false;
        _raised = value;
    }
}