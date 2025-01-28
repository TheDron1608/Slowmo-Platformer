using System;
using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    const string ANIMATOR_ATTACK_TRIGGER_NAME = "Attack";
    const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";

    [Header("Weapon")]
    [SerializeField] private float _attackCooldown = .25f;
    [SerializeField] private float _attackCooldownMultiplier = 1f;
    public bool PlayerInputAutoAttackOnPress = false;
    public AbstractProjectile Projectile;
    public float AccuracyMultiplier = 1f;

    protected Animator _animator;

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
        Vector2 currentDirection;
        if (TryGetComponent(out Holdable holdable) && holdable.RotatableWhenIsHolded)
        {
            currentDirection = VectorMath.Quartenion2DToVec2(holdable.transform.rotation);
        }
        else
        {
            currentDirection = direction;
        }

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
        StartCoroutine(AwaitAttackCooldownFinish());

        //knockback
        if (TryGetComponent(out Holdable holdable))
        {
            if (holdable.CurrentHolder.TryGetComponent(out Rigidbody2D rigidBody))
            {

                rigidBody.linearVelocity -= direction * Projectile.KnockBack;

                if (holdable.CurrentHolder.TryGetComponent(out CharacterVisual charVisual))
                {
                    charVisual.SpritesFlipped = direction.x < 0f;
                }
            }
        }

        Projectile.SpawnProjectile(direction, AccuracyMultiplier, gameObject.GetComponent<Weapon>());
        _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER_NAME);

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
        yield return new WaitForSeconds(AttackCooldown * AttackCooldownMultiplier);
        IsAbleToAttack = true;
    }
}
