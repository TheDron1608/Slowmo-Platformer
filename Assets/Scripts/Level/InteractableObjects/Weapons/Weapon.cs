using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IEffectApplier
{
    public enum WEAPON_TAGS
    {
        MELEE = 0,
        RANGED = 1,

        PISTOL = 10,
        REVOLVER = 11,
        SHOTGUN = 12,
        RIFLE = 13,
        MACHINE_GUN = 14,

        BROKEN = 20,
        POCKET = 21,
        MEDIUM = 22,
        HEAVY = 23,

        SINGLE_ATTACKING = 30,
        SEMI_AUTO = 31,
        FULL_AUTO = 32,
        BURST = 33
    }

    protected const string PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME = "ProjectileSpawnPosition";

    [Header("Weapon")]
    public WEAPON_TAGS[] Tags = new WEAPON_TAGS[0];
    [SerializeField] private float _attackCooldown = .25f;
    [SerializeField] private float _attackCooldownMultiplier = 1f;
    public bool AutoAttack = false;
    public AbstractProjectile Projectile;
    public float AccuracyMultiplier = 1f;
    public int RepeatAttacksTimes = 1;
    public float DurationBetweenRepeatAttacks = 0.0667f; //in seconds
    public List<AbstractEffect> ExtraProjectileEffects = new();
    public bool IsAbleToAttack = true;

    private bool _isInCooldown = false;
    private Transform _projectileSpawnPosition;
    private List<AbstractProjectile> _projectiles = new();
    private Coroutine _awaitAttackCooldownCoroutine = null;

    public event EventHandler<IEffectApplier.OnEffectAppliedEventArgs> OnEffectApplied;
    public event EventHandler OnAttackSucceed;
    public event EventHandler OnAttackFailed;

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

    public bool IsInCooldown
    {
        get => _isInCooldown;
        set
        {
            _isInCooldown = value;
        }
    }

    public List<AbstractProjectile> Projectiles
    {
        get => _projectiles;
        set => _projectiles = value;
    }

    public Transform ProjectileSpawnPosition
    {
        get => _projectileSpawnPosition;
        protected set => _projectileSpawnPosition = value;
    }

    public virtual void ResetAttackCooldown()
    {
        IsInCooldown = false;
        if (_awaitAttackCooldownCoroutine != null)
        {
            StopCoroutine(_awaitAttackCooldownCoroutine);
            _awaitAttackCooldownCoroutine = null;
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    private void OnEnable()
    {
        VirtualOnEnable();
    }

    protected virtual void OnAwake()
    {
        _projectileSpawnPosition = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME);
    }

    protected virtual void VirtualOnEnable()
    {
        IsInCooldown = false;
    }

    public Vector2 GetCurrentAvaibleAim()
    {
        if (TryGetComponent(out Holdable holdableweapon) && holdableweapon.CurrentHolder != null && holdableweapon.CurrentHolder.TryGetComponent(out CharacterAiming holderAiming))
        {
            return holderAiming.GetCurrentAimNormalized();
        }
        else
        {
            return VectorMath.Quartenion2DToVec3(transform.rotation);
        }
    }

    public virtual bool GetIsAbleToAttack()
    {
        return true;
    }

    public bool TryAttack(Vector2 direction, bool ignoreCooldown = false)
    {
        if (AttackCondition() && (ignoreCooldown || !IsInCooldown))
        {
            StartCoroutine(AttackMultipleTimes());
            return true;
        }
        else
        {
            Vector2 currentDirection;
            if (TryGetComponent(out Holdable holdable) && holdable.RotatableWhenIsHolded)
            {
                currentDirection = VectorMath.Quartenion2DToVec3(holdable.transform.rotation);
            }
            else
            {
                currentDirection = direction;
            }

            OnTryAttackFail(direction);
            return false;
        }
    }

    public bool TrySingleAttack(Vector2 direction, bool ignoreCooldown = false)
    {
        Vector2 currentDirection;
        if (TryGetComponent(out Holdable holdable) && holdable.RotatableWhenIsHolded)
        {
            currentDirection = VectorMath.Quartenion2DToVec3(holdable.transform.rotation);
        }
        else
        {
            currentDirection = direction;
        }

        if (AttackCondition() && (ignoreCooldown || !IsInCooldown))
        {
            OnTryAttackSuccess(currentDirection);
            return true;
        }
        else
        {
            OnTryAttackFail(currentDirection);
            return false;
        }
    }

    public bool IsReadyToAttack()
    {
        return AttackCondition();
    }

    protected virtual bool OnTryAttackSuccess(Vector2 direction)
    {
        OnAttackSucceed?.Invoke(this, EventArgs.Empty);

        IsInCooldown = true;

        List<AbstractProjectile> newProjectiles = Projectile.SpawnProjectile(
            direction,
            transform.position,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject),
            this,
            AccuracyMultiplier
            );

        _projectiles.AddRange(newProjectiles);
        for (int i = 0; i < newProjectiles.Count; i++)
        {
            newProjectiles[i].OnHitSomeOne += NewProjectile_OnHitSomething;
            newProjectiles[i].OnDestroyed += NewProjectile_OnDestroyed;
        }

        return true;
    }

    private void NewProjectile_OnHitSomething(object sender, GameObject e)
    {
        (sender as AbstractProjectile).OnHitSomeOne -= NewProjectile_OnHitSomething;
        (sender as AbstractProjectile).OnDestroyed -= NewProjectile_OnDestroyed;

        if (
            this != null && !this.IsDestroyed() && this.TryGetComponent(out BreakableHoldable breakableHoldable) && 
            breakableHoldable != null && !breakableHoldable.IsDestroyed()
            )
        {
            breakableHoldable.SpendOneUse();
        }
    }
    private void NewProjectile_OnDestroyed(object sender, EventArgs e)
    {
        (sender as AbstractProjectile).OnHitSomeOne -= NewProjectile_OnHitSomething;
        (sender as AbstractProjectile).OnDestroyed -= NewProjectile_OnDestroyed;
    }


    protected virtual void OnTryAttackFail(Vector2 direction)
    {
        OnAttackFailed?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator AttackMultipleTimes()
    {
        int attackRepeatsLeft = RepeatAttacksTimes;
        while (true)
        {
            if (!TrySingleAttack(GetCurrentAvaibleAim(), true)) break;

            attackRepeatsLeft--;

            if (attackRepeatsLeft <= 0) break;

            yield return new WaitForSeconds(DurationBetweenRepeatAttacks);
        }
        _awaitAttackCooldownCoroutine = StartCoroutine(AwaitAttackCooldownFinish());
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
        yield return new WaitForSeconds(AttackCooldown * AttackCooldownMultiplier);
        IsInCooldown = false;
        _awaitAttackCooldownCoroutine = null;
    }

    public virtual void InvokeOnEffectApllied(AbstractEffect Effect, ObjectEffectsReceiver Receiver)
    {
        OnEffectApplied?.Invoke(this, new(this, Effect, Receiver));
        if (!gameObject.IsDestroyed())
        {
            GetComponent<Holdable>()?.CurrentOrLastHolder?.CharComponents.CharacterAttacking?.InvokeOnEffectApllied(Effect, Receiver);
        }
    }
}
