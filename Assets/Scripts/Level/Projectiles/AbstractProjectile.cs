using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour, IEffectApplier
{
    const string ANIMATOR_RESET_TRIGGER_NAME = "Reset";
    const float MAX_ATTACK_SOUND_PITCH_ON_LOW_AMMO = 2f;

    public int AmountOnSpawn = 1;
    public int HitAmountOnSingleTargetForExtraEffects = 1;
    public float Accuracy = 1f;
    public List<AbstractEffect> HitEffects = new();
    public List<AbstractEffect> SelfEffects = new();
    public List<AbstractEffect> SelfEffectsOnWeapon = new();
    public List<AbstractEffect> ExtraEffectsOnAllProjectilesHitSingleTarget = new();
    public bool FriendlyFire = false;
    public bool IsAbleToHit = true;
    public Sprite GameplayUISprite;
    public AbstractSoundPlayer SoundOnAttack;
    public AbstractSoundPlayer SoundOnBlocked;
    public CharacterVisual.CharacterPartBusyStates UnarmedAttackAnimation = CharacterVisual.CharacterPartBusyStates.NONE;

    private Weapon _weapon = null;
    private CharacterHoldingObjects _deflector = null;
    private CharacterHoldingObjects _owner = null;
    protected List<Collider2D> _currentHittingColliders = new();
    protected bool _wasDeflectedThisFrame = false;
    protected bool _failedPierceThisFrame = false;
    private ObjectEffectsReceiver _effectsReceiver;
    private BoxCollider2D _colliderComponent;
    private List<AbstractEffect> _extraEffectsFromWeapon = new();
    private List<AbstractEffect> _extraEffectsFromOwner = new();
    private List<AbstractProjectile> _multitSpawnProjectiles = new();
    private List<GameObject> _hitObjects = new();

    public event EventHandler<GameObject> OnHitSomeOne;
    public event EventHandler OnDestroyed;
    public event EventHandler<IEffectApplier.OnEffectAppliedEventArgs> OnEffectApplied;

    public float ProjectileSize
    {
        get => GetComponent<BoxCollider2D>().size.x;
    }

    public bool WasDeflectedThisFrame
    {
        get => _wasDeflectedThisFrame;
    }

    public bool WasResistedDamageThisFrame
    {
        get => _failedPierceThisFrame;
    }

    public List<GameObject> HitObjects
    {
        get => _hitObjects;
    }

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (!TryGetComponent(out _effectsReceiver)) throw new UnityException("ObjectEffectsReceiver component not found at " + gameObject.name);
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("BoxCollider2D component not found at " + gameObject.name);
    }

    public virtual Weapon Weapon
    {
        get => _weapon;
        protected set
        {
            if (_weapon == value) return;
            _weapon = value;

            _effectsReceiver.RemoveEffect(_extraEffectsFromWeapon);
            if (_weapon != null)
            {
                _extraEffectsFromWeapon = _effectsReceiver.ApplyEffect(_weapon.ExtraProjectileEffects, _weapon);
            }
        }
    }
    public CharacterHoldingObjects Deflector
    {
        get => _deflector;
        protected set
        {
            if (_deflector == value) return;
            _deflector = value;

            _effectsReceiver.RemoveEffect(_extraEffectsFromOwner);
            if (_deflector != null)
            {
                _extraEffectsFromOwner = _effectsReceiver.ApplyEffect(_deflector.CharComponents.CharacterAttacking.ExtraProjectileEffects, _deflector);
            }
        }
    }
    public CharacterHoldingObjects Owner
    {
        get => _owner;
        set
        {
            if (_owner == value) return;
            _owner = value;

            _effectsReceiver.RemoveEffect(_extraEffectsFromOwner);
            if (_owner != null)
            {
                _extraEffectsFromOwner = _effectsReceiver.ApplyEffect(_owner.CharComponents.CharacterAttacking.ExtraProjectileEffects, _owner);
            }
        }
    }
    public CharacterHoldingObjects OwnerOrLastHolder
    {
        get
        {
            if (Owner != null && !Owner.IsDestroyed()) return Owner;
            else if (
                Weapon != null && !Weapon.IsDestroyed() &&
                Weapon.TryGetComponent(out Holdable holdableWeapon) && !holdableWeapon.IsDestroyed()
                ) return holdableWeapon.LastHolder;
            else return null;
        }
    }
    public ObjectEffectsReceiver EffectsReceiver
    {
        get => _effectsReceiver;
    }

    public List<AbstractProjectile> SpawnProjectile(Vector2 direction, Vector2 position, ZIndexLayer layer, Weapon weapon = null, float accuracityMultiplier = 1f)
    {
        return SpawnProjectile(VectorMath.Vec2ToQuarterninon2D(direction), position, layer, weapon, accuracityMultiplier);
    }

    public List<AbstractProjectile> SpawnProjectile(Quaternion direction, Vector2 position, ZIndexLayer layer, Weapon weapon = null, float accuracityMultiplier = 1f)
    {
        List<AbstractProjectile> result = new(AmountOnSpawn);

        for (int i = 0; i < AmountOnSpawn; i++)
        {
            AbstractProjectile newProjectile = ProjectilesManager.Instance.GetUnusedProjectile(this);
            newProjectile.SetAttrs(this, VectorMath.RandomizeQuarternion(direction, Accuracy * accuracityMultiplier), position, layer, weapon);
            newProjectile._multitSpawnProjectiles = result;
            result.Insert(i, newProjectile);
        }

        if (weapon != null && result.Count > 0)
        {
            if (weapon is RangedWeapon rangedWeapon)
            {
                result[0].SoundOnAttack.Pitch = math.lerp(
                    1f, 
                    MAX_ATTACK_SOUND_PITCH_ON_LOW_AMMO, 
                    math.cos((float)rangedWeapon.LoadedLivingAmmoLeft / rangedWeapon.GetAmmoCapacity() * math.PIHALF)
                    );
            }
            else
            {
                result[0].SoundOnAttack.Pitch = 1f;
            }

            result[0].SoundOnAttack.PlaySound(
                weapon.OverrideAttackSound ?? result[0].SoundOnAttack.DefaultSound,
                false, 
                weapon.ProjectileSpawnPosition.transform.position
                );
        }

        ApplySelfEffectOnWeaponUserOrWeapon(result, weapon);

        return result;
    }

    protected virtual void SetAttrs(AbstractProjectile original, Quaternion direction, Vector2 position, ZIndexLayer layer, Weapon weapon)
    {
        transform.rotation = direction;
        if (weapon != null)
        {
            transform.position = weapon.ProjectileSpawnPosition.transform.position;
        }
        else
        {
            transform.position = new Vector3(position.x, position.y, layer.transform.position.z);
        }

        gameObject.SetActive(true);

        _effectsReceiver.RemoveAllEffects();
        _currentHittingColliders = new();
        _wasDeflectedThisFrame = false;
        _extraEffectsFromOwner = new();
        _extraEffectsFromWeapon = new();
        _weapon = null;
        _owner = null;
        _deflector = null;
        _multitSpawnProjectiles = new();
        _hitObjects = new();

        gameObject.name = original.gameObject.name;
        AmountOnSpawn = original.AmountOnSpawn;
        Accuracy = original.Accuracy;
        HitEffects = original.HitEffects;
        SelfEffects = original.SelfEffects;
        ExtraEffectsOnAllProjectilesHitSingleTarget = original.ExtraEffectsOnAllProjectilesHitSingleTarget;
        FriendlyFire = original.FriendlyFire;
        IsAbleToHit = original.IsAbleToHit;
        HitAmountOnSingleTargetForExtraEffects = original.HitAmountOnSingleTargetForExtraEffects;
        UnarmedAttackAnimation = original.UnarmedAttackAnimation;

        Animator animator = GetComponent<Animator>();
        Animator originalAnimator = original.GetComponent<Animator>();
        animator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
        animator.SetTrigger(ANIMATOR_RESET_TRIGGER_NAME);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        SpriteRenderer originalSpriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = originalSpriteRenderer.sprite;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        BoxCollider2D originalBoxCollider = original.GetComponent<BoxCollider2D>();
        boxCollider.size = originalBoxCollider.size;
        boxCollider.offset = originalBoxCollider.offset;

        SoundOnAttack.DefaultSound = original.SoundOnAttack.DefaultSound;
        SoundOnAttack.Volume = original.SoundOnAttack.Volume;
        SoundOnAttack.Pitch = original.SoundOnAttack.Pitch;

        SoundOnBlocked.DefaultSound = original.SoundOnBlocked.DefaultSound;
        SoundOnBlocked.Volume = original.SoundOnBlocked.Volume;
        SoundOnBlocked.Pitch = original.SoundOnBlocked.Pitch;

        LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);
    }

    protected void InitEffects(AbstractProjectile original, Weapon weapon)
    {
        _effectsReceiver.ApplyEffect(original.GetComponent<ObjectEffectsReceiver>().CurrentEffects, null);
        Weapon = weapon;
        if (weapon?.TryGetComponent(out Holdable holdableWeapon) ?? false)
        {
            Owner = holdableWeapon.CurrentHolder ?? holdableWeapon.LastHolder;
        }
        else if (weapon?.TryGetComponent(out UnarmedWeapon unarmedWeapon) ?? false)
        {
            Owner = unarmedWeapon.CharComponents.CharacterHolding;
        }
    }


    private void LateUpdate()
    {
        OnLateUpdate();
    }

    protected virtual void OnLateUpdate()
    {
        _wasDeflectedThisFrame = false;
        _failedPierceThisFrame = false;
    }

    protected void AddCurrentHittingCollidersItem(Collider2D item)
    {
        if (item.TryGetComponent(out AbstractCharacterComponent charCollider))
        {
            _currentHittingColliders.Add(charCollider.CharComponents.CharacterRigidBodyCapsuleCollider);
            foreach (CharacterPart charPart in charCollider.CharComponents.CharacterPartsManager.CharacterParts)
            {
                Collider2D charPartCollider = charPart.GetComponentInChildren<Collider2D>();
                if (charPartCollider != null)
                {
                    _currentHittingColliders.Add(charPartCollider);
                }
            }
        }
        else
        {
            _currentHittingColliders.Add(item);
        }
    }

    public virtual void OnDeflected(MonoBehaviour deflector)
    {
        if (deflector != null)
        {
            if (deflector.TryGetComponent(out AbstractProjectile projectile)) Deflector = projectile.OwnerOrLastHolder;
            else if (deflector.TryGetComponent(out Holdable holdable)) Deflector = holdable.CurrentOrLastHolder;
            else if (deflector.TryGetComponent(out AbstractCharacterComponent charComponent)) Deflector = charComponent.CharComponents.CharacterHolding;
        }

        _wasDeflectedThisFrame = true;
    }

    private void ApplySelfEffectOnWeaponUserOrWeapon(List<AbstractProjectile> projectiles, Weapon weapon)
    {
        if (weapon != null)
        {
            if (
                weapon.TryGetComponent(out Holdable holdableWeapon) &&
                holdableWeapon?.CurrentHolder != null &&
                holdableWeapon.CurrentHolder.TryGetComponent(out CharacterComponentsManager holderCharComponents)
                )
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    holderCharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffects, projectiles[i], 1f, true);
                }
            }
            else if (weapon.TryGetComponent(out UnarmedWeapon unarmedWeapon))
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    unarmedWeapon.CharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffects, projectiles[i], 1f, true);
                }
            }

            if (weapon.TryGetComponent(out ObjectEffectsReceiver weaponEffectsReceiver))
            {
                for (int i = 0; i < projectiles.Count; i++)
                {
                    weaponEffectsReceiver.ApplyEffect(SelfEffectsOnWeapon, projectiles[i], 1f, true);
                }
            }
        }
    }

    public virtual void OnHit(GameObject hitObject)
    {
        _hitObjects.Add(hitObject);

        if (hitObject.TryGetComponent(out MeleeProjectile meleeProjectile) && meleeProjectile.DeflectCondition(this))
        {
            meleeProjectile.OnDeflect(this);
        }

        if (GameObjectUtility.TryGetComponentInSelfOrParent(hitObject.gameObject, out ObjectEffectsReceiver hitObjectEffectsReceiver))
        {
            List<AbstractEffect> appliedEffects = hitObjectEffectsReceiver.ApplyEffect(HitEffects, this);

            if (HitEffects.Any(e => e is Damage) && !appliedEffects.Any(e => e is Damage))
            {
                _failedPierceThisFrame = true;
            }

            if (_multitSpawnProjectiles.Count(e => e.HitObjects.Contains(hitObject)) == HitAmountOnSingleTargetForExtraEffects)
            {
                hitObjectEffectsReceiver.ApplyEffect(ExtraEffectsOnAllProjectilesHitSingleTarget, this, 1f, true);
            }

            if (!WasDeflectedThisFrame)
            {
                OnHitSomeOne?.Invoke(this, hitObject);
            }
        }

        if (GameObjectUtility.TryGetComponentInSelfOrParent(hitObject.gameObject, out IDamagable hitDamagableObject))
        {
            hitDamagableObject.ApplyProjectileHit(this, WasDeflectedThisFrame || WasResistedDamageThisFrame);
        }
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
    }

    protected virtual bool HitCondition(List<Collider2D> totalHitObjects, Collider2D currentHitObjet)
    {
        //returns true if:
        //1. if not deflected this frame
        //2. hit object is not weapon's owner
        //3. hit object is not weapon's owner's team ally
        //4. has the highest hit priority at all hit character's parts (if hit multiple character's parts)
        //5. did not already hit this object this frame
        return
            gameObject.activeSelf &&
            !_wasDeflectedThisFrame &&
            (
                !currentHitObjet.TryGetComponent(out AbstractProjectile projectile) ||
                Weapon != projectile.Weapon
            ) &&
            (
                !currentHitObjet.TryGetComponent(out Shield shield) ||
                FriendlyFire || (!shield.GetComponent<Holdable>().CurrentHolder?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam((Deflector ?? Owner)?.CharComponents.CharacterTeam) ?? true)
            ) &&
            (
                !currentHitObjet.TryGetComponent(out AbstractCharacterComponent charComponent) ||
                (
                    charComponent.CharComponents.CharacterHolding != (Deflector ?? Owner) &&
                    (FriendlyFire || !charComponent.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam((Deflector ?? Owner)?.CharComponents.CharacterTeam))
                )
            ) &&
            (
                !currentHitObjet.TryGetComponent(out CharacterHitbox charHitbox) ||
                (
                    charHitbox.HitableByProjectiles &&
                    GetIsHighestHitPriority(totalHitObjects, charHitbox) &&
                    currentHitObjet.transform.parent.TryGetComponent(out CharacterPart charPart)
                )
            ) &&
            !_currentHittingColliders.Contains(currentHitObjet);
    }

    private bool GetIsHighestHitPriority(List<Collider2D> colliders, CharacterHitbox currentHitBox)
    {
        int currentHighestPriority = currentHitBox.HitPriority;
        for (int i = 0; i < colliders.Count; i++)
        {
            if (
                colliders[i].TryGetComponent(out CharacterHitbox charHitbox) &&
                AbstractCharacterComponent.GetCharacterComponentsEqual(currentHitBox, charHitbox)
                )
            {
                currentHighestPriority = Mathf.Max(currentHighestPriority, charHitbox.HitPriority);
            }
        }

        return currentHighestPriority <= currentHitBox.HitPriority;
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this, EventArgs.Empty);
    }

    public virtual void RemoveProjectile()
    {
        if (Weapon != null)
        {
            Weapon.Projectiles.Remove(this);
        }
        gameObject.SetActive(false);
    }

    public void InvokeOnEffectApllied(AbstractEffect Effect, ObjectEffectsReceiver Receiver)
    {
        OnEffectApplied?.Invoke(this, new(this, Effect, Receiver));
        if (Deflector != null)
        {
            Deflector?.CharComponents.CharacterAttacking?.InvokeOnEffectApllied(Effect, Receiver);
        }
        else if (Weapon != null && !Weapon.IsDestroyed())
        {
            Weapon?.InvokeOnEffectApllied(Effect, Receiver);
        }
        else if (Owner != null)
        {
            Owner?.CharComponents.CharacterAttacking?.InvokeOnEffectApllied(Effect, Receiver);
        }
    }
}
