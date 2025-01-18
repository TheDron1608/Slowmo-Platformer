using System.Collections;
using UnityEngine;

public abstract class Weapon : Holdable
{
    const string ANIMATOR_ATTACK_TRIGGER_NAME = "Attack";

    public enum AttackPiercing
    {
        NO_PIERCE,
        PIERCE_ARMOR,
        PIERCE_HEAVY_ARMOR
    }

    [Header("Weapon")]
    public float Damage = 1f;
    public float KnockBack = 0f;
    public float BaseAttackCoolDownSeconds = 0.5f;
    [SerializeField] private float _attackCooldownMultiplier = 1f;
    public float MaxRange = 350f;
    public bool PlayerInputAutoAttackOnPress = false;
    public AttackPiercing Pierce = AttackPiercing.NO_PIERCE;


    private float _attackCooldown = 0f;
    private bool _isAbleToAttack = true;

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

    public virtual float AttackCooldownMultiplier
    {
        get => _attackCooldownMultiplier;
        set => _attackCooldownMultiplier = value;
    }

    public bool IsAbleToAttack
    {
        get => _isAbleToAttack;
        set => _isAbleToAttack = value;
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
        if (!AttackCondition()) return;

        _attackCooldown = BaseAttackCoolDownSeconds * AttackCooldownMultiplier;
        OnTryAttack();

        StartCoroutine(AwaitAttackCooldownFinish());
    }

    protected virtual bool OnTryAttack()
    {
        if (!AttackCondition()) return false;

        _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER_NAME);
        return true;
    }

    protected virtual bool AttackCondition()
    {
        return IsAbleToAttack;
    }

    protected virtual void OnFinishAttack()
    {

    }

    private IEnumerator AwaitAttackCooldownFinish()
    {
        IsAbleToAttack = false;
        while (_attackCooldown > 0f)
        {
            yield return new WaitForEndOfFrame();

            _attackCooldown -= Time.deltaTime;
        }
        _attackCooldown = 0f;
        IsAbleToAttack = true;
        OnFinishAttack();
    }
}
