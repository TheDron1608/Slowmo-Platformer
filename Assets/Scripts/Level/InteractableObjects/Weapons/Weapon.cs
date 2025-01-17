using System.Collections;
using UnityEngine;

public abstract class Weapon : Holdable
{
    const string ANIMATOR_AUTO_ATTACK_PROP_NAME = "AutoAttack";

    public enum AttackPiercing
    {
        NO_PIERCE,
        PIERCE_ARMOR,
        PIERCE_HEAVY_ARMOR
    }

    public float Damage = 1f;
    public float KnockBack = 0f;
    public float BaseAttackCoolDownSeconds = 0.5f;
    public float MaxRange = 350f;
    public bool PlayerInputAutoAttackOnPress = false;
    public AttackPiercing Pierce = AttackPiercing.NO_PIERCE;

    private float _attackCooldown = 0f;

    private bool _autoAttack = false;

    public float AttackCooldown
    {
        get => _attackCooldown;
        set 
        {
            if (_attackCooldown > 0f && value <= 0f) OnFinishAttack();
            _attackCooldown = value;
        }
    }

    public bool AutoAttack
    {
        get => _autoAttack;
        set
        {
            _animator.SetBool(ANIMATOR_AUTO_ATTACK_PROP_NAME, value);
            _autoAttack = value;
        }
    }

    protected Animator _animator;

    private void Awake()
    {
        OnAwake();
    }

    protected new virtual void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found");
    }

    protected override void OnThrow()
    {
        base.OnThrow();
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();
        if (LastHolder != CurrentHolder)
        {
            _attackCooldown = 0f;
        }
    }

    public void TryAttack(Vector2 direction)
    {
        if (_attackCooldown > 0f) return;

        _attackCooldown = BaseAttackCoolDownSeconds;
        OnTryAttack();

        StartCoroutine(AwaitAttackCooldownFinish());
    }

    protected virtual bool OnTryAttack()
    {
        return AttackCondition();
    }

    protected virtual bool AttackCondition()
    {
        return true;
    }

    protected virtual void OnFinishAttack()
    {

    }

    private IEnumerator AwaitAttackCooldownFinish()
    {
        while (_attackCooldown > 0f)
        {
            yield return new WaitForEndOfFrame();

            _attackCooldown -= Time.deltaTime;
        }
        _attackCooldown = 0f;
        OnFinishAttack();
    }
}
