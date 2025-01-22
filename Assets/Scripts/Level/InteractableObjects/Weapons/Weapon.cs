using System;
using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    const string ANIMATOR_ATTACK_TRIGGER_NAME = "Attack";
    public const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";

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

    protected Animator _animator;

    private float _attackCooldown = 0f;
    private bool _isAbleToAttack = true;
    private bool _isThrown = true;

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

    public bool IsThrown
    {
        get => _isThrown;
        set
        {
            _animator.SetBool(ANIMATOR_ISTHROWN_PROP_NAME, value);
            _isThrown = value;
        }
    }

    protected void CallAnimatorAttackTrigger()
    {
        _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER_NAME);
    }


    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (!TryGetComponent(out _animator)) throw new UnityException("Animator component not found");
    }

    public bool TryAttack(Vector2 direction)
    {
        if (AttackCondition())
        {
            OnTryAttackSuccess(direction);
            return true;
        }
        else
        {
            OnTryAttackFail(direction);
            return false;
        }
    }

    protected virtual bool OnTryAttackSuccess(Vector2 direction)
    {

        //attack cooldown
        _attackCooldown = BaseAttackCoolDownSeconds * AttackCooldownMultiplier;
        StartCoroutine(AwaitAttackCooldownFinish());

        //knockback
        if (TryGetComponent(out Holdable holdable))
        {
            if (holdable.CurrentHolder.TryGetComponent(out Rigidbody2D rigidBody))
            {

                Vector2 aimDirection = direction;

                rigidBody.linearVelocity += aimDirection * KnockBack;

                if (holdable.CurrentHolder.TryGetComponent(out CharacterVisual charVisual))
                {
                    charVisual.SpritesFlipped = aimDirection.x < 0f;
                }
            }
        }

        return true;
    }

    protected virtual void OnTryAttackFail(Vector2 direction)
    {

    }

    protected virtual bool AttackCondition()
    {
        return IsAbleToAttack;
    }

    /// <summary>
    /// Call only from animator or as private
    /// </summary>
    public virtual void OnFinishAttack()
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
