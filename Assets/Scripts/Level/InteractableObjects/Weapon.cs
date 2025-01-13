using System.Collections;
using UnityEngine;

public abstract class Weapon : Holdable
{
    const string ANIMATOR_ATTACK_TRIGGER_NAME = "Attack";
    const string ANIMATOR_AUTO_ATTACK_PROP_NAME = "AutoAttack";
    const string ANIMATOR_IS_THROWN_PROP_NAME = "IsThrown";

    public enum AttackPiercing
    {
        NO_PIERCE,
        PIERCE_ARMOR,
        PIERCE_HEAVY_ARMOR
    }

    public float Damage = 1f;
    public float KnockBack = 0f;
    public float BaseAttackCoolDownSeconds = 0.5f;
    public bool PlayerInputAutoAttackOnPress = false;
    public AttackPiercing Pierce = AttackPiercing.NO_PIERCE;
    public Projectile Projectile;

    private float _attackCooldown = 0f;

    private bool _autoAttack = false;
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

    public bool AutoAttack
    {
        get => _autoAttack;
        set
        {
            _animator.SetBool(ANIMATOR_AUTO_ATTACK_PROP_NAME, value);
            _autoAttack = value;
        }
    }

    public bool IsThrown
    {
        get => _isThrown;
        set
        {
            _animator.SetBool(ANIMATOR_IS_THROWN_PROP_NAME, value);
            _isThrown = value;
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
        IsThrown = true;
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();
        IsThrown = false;
        if (LastHolder != CurrentHolder)
        {
            _attackCooldown = 0f;
        }
    }

    public void Attack(Vector2 direction)
    {
        if (_attackCooldown > 0f) return;

        _attackCooldown = BaseAttackCoolDownSeconds;
        OnAttack();

        StartCoroutine(AwaitAttackCooldownFinish());
    }

    protected virtual void OnAttack()
    {
        _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER_NAME);

        Projectile projectile = Instantiate(Projectile, transform);

        if (CurrentHolder.TryGetComponent(out CharacterAiming characterAiming))
        {
            projectile.transform.LookAt(characterAiming.CurrentAimPoint);
            projectile.transform.rotation = VectorMath.Vec3ToQuarterninon2D(characterAiming.GetCurrentAimNormalized());
        }
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
