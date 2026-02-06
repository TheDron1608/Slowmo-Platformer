using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class Shield : DamagableObject, IThrowableIteractableObj
{
    const string ANIMATOR_RAISED_PROP_NAME = "Raised";
    const string ANIMATOR_IS_THROWN_PROP_NAME = "IsThrown";
    const int ORDER_IN_LAYER_ON_RAISED_CHNGE = 200;

    public List<AbstractEffect> HolderEffectsOnRaise = new();
    public List<AbstractEffect> SelfEffectsOnRaise = new();

    private Animator _animatorComponent;
    private Holdable _holdableComponent;
    private bool _raised = false;
    private bool _isRaisingUp = false;
    private bool _isRaisingDown = false;
    private float _defaultHoldDistance;
    private bool _isThrown = true;

    public bool IsThrown
    {
        get => _isThrown;
        set
        {
            if (_isThrown == value) return;

            _isThrown = value;
            _animatorComponent.SetBool(ANIMATOR_RAISED_PROP_NAME, false);
            _animatorComponent.SetBool(ANIMATOR_IS_THROWN_PROP_NAME, value);
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _animatorComponent)) throw new UnityException("Animator component not found at " + gameObject.name);
        if (!TryGetComponent(out _holdableComponent)) throw new UnityException("Holdable component not found at " + gameObject.name);

        _defaultHoldDistance = _holdableComponent.HoldDistanceWhenIsHolded;
        _holdableComponent.GetComponent<Renderer>().sortingOrder -= ORDER_IN_LAYER_ON_RAISED_CHNGE;
        _holdableComponent.HoldDistanceWhenIsHolded = 0f;
    }

    public bool Raised
    {
        get => _raised;
        set
        {
            if (_raised == value) return;
            _raised = value;
            _holdableComponent.HitableWhenIsHolded = value;
            _holdableComponent.RotatableWhenIsHolded = value;
            _holdableComponent.HoldDistanceWhenIsHolded = value ? _defaultHoldDistance : 0f;
            _holdableComponent.GetComponent<Renderer>().sortingOrder += value ? ORDER_IN_LAYER_ON_RAISED_CHNGE : -ORDER_IN_LAYER_ON_RAISED_CHNGE;

            GetComponent<BreakableObject>()?.RemoveAllStuckedObjects();

            if (_raised)
            {
                _holdableComponent.CurrentHolder?.CharComponents.CharacterEffectsReceiver.ApplyEffect(HolderEffectsOnRaise, this, 1f, true);
                GetComponent<ObjectEffectsReceiver>()?.ApplyEffect(SelfEffectsOnRaise, this, 1f, true);
            }
            else
            {
                _holdableComponent.CurrentOrLastHolder?.CharComponents.CharacterEffectsReceiver.RemoveEffect(HolderEffectsOnRaise);
                GetComponent<ObjectEffectsReceiver>()?.RemoveEffect(SelfEffectsOnRaise);
            }
        }
    }

    public bool IsRaisingUp
    {
        get => _isRaisingUp;
        private set => _isRaisingUp = value;
    }

    public bool IsRaisingDown
    {
        get => _isRaisingDown;
        private set => _isRaisingDown = value;
    }

    public bool GetIsRaising()
    {
        return IsRaisingDown || IsRaisingUp;
    }

    public bool TryRaiseUp()
    {
        _animatorComponent.SetBool(ANIMATOR_RAISED_PROP_NAME, true);
        IsRaisingUp = true;
        IsRaisingDown = false;

        return true;
    }

    public bool TryRaiseDown()
    {
        _animatorComponent.SetBool(ANIMATOR_RAISED_PROP_NAME, false);
        IsRaisingDown = true;
        IsRaisingUp = false;

        return true;
    }

    public void Animator_OnRaisedChanged(bool value)
    {
        IsRaisingUp = false;
        IsRaisingDown = false;
        Raised = value;
    }
}