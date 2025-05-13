using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ThrowableWeapon : Weapon
{
    const string ANIMATOR_ATTACK_TRIGGER_NAME = "Attack";
    const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";

    protected Animator _animator;

    private bool _isThrown = true;

    public bool IsThrown
    {
        get => _isThrown;
        set
        {
            _animator.SetBool(ANIMATOR_ISTHROWN_PROP_NAME, value);
            _isThrown = value;
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found");
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        base.OnTryAttackSuccess(direction);
        _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER_NAME);

        return true;
    }
}
