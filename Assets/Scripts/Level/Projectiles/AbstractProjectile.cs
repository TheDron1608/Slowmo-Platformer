using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour, IEffectApplier
{
    const string ANIMATOR_RESET_TRIGGER_NAME = "Reset";

    public int AmountOnSpawn = 1;
    public float Accuracy = 1f;
    public List<AbstractEffect> HitEffects = new();
    public List<AbstractEffect> SelfEffects = new();
    public List<AbstractEffect> SelfEffectsOnWeapon = new();
    public bool FriendlyFire = false;
    public bool IsAbleToHit = true;
    public Sprite GameplayUISprite;
    public AbstractSoundPlayer SoundOnAttack;

    private Weapon _weapon = null;
    private CharacterHoldingObjects _deflector = null;
    private CharacterHoldingObjects _owner = null;
    protected List<Collider2D> _currentHittingColliders = new();
    protected bool _wasDeflectedThisFrame = false;
    private ObjectEffectsReceiver _effectsReceiver;
    private BoxCollider2D _colliderComponent;
    private List<AbstractEffect> _extraEffectsFromWeapon = new();
    private List<AbstractEffect> _extraEffectsFromOwner = new();

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
        //get => Owner ?? Weapon?.GetComponent<Holdable>()?.LastHolder;
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
            result.Insert(i, newProjectile);
        }

        if (weapon != null && result.Count > 0)
        {
            result.First().SoundOnAttack.PlaySound(false, weapon.ProjectileSpawnPosition.transform.position);
        }

        ApplySelfEffectOnWeaponUserOrWeapon(result, weapon);

        return result;
    }

    protected virtual void SetAttrs(AbstractProjectile original, Quaternion direction, Vector2 position, ZIndexLayer layer, Weapon weapon)
    {
        gameObject.SetActive(true);

        _effectsReceiver.RemoveAllEffects();
        _currentHittingColliders = new();
        _wasDeflectedThisFrame = false;
        _extraEffectsFromOwner = new();
        _extraEffectsFromWeapon = new();
        _weapon = null;
        _owner = null;
        _deflector = null;

        transform.rotation = direction;
        transform.position = weapon.ProjectileSpawnPosition.transform.position;

        gameObject.name = original.gameObject.name;
        AmountOnSpawn = original.AmountOnSpawn;
        Accuracy = original.Accuracy;
        HitEffects = original.HitEffects;
        SelfEffects = original.SelfEffects;
        FriendlyFire = original.FriendlyFire;
        IsAbleToHit = original.IsAbleToHit;

        Animator animator = GetComponent<Animator>();
        Animator originalAnimator = original.GetComponent<Animator>();
        animator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
        animator.SetTrigger(ANIMATOR_RESET_TRIGGER_NAME);

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        BoxCollider2D originalBoxCollider = original.GetComponent<BoxCollider2D>();
        boxCollider.size = originalBoxCollider.size;
        boxCollider.offset = originalBoxCollider.offset;

        SoundOnAttack.DefaultSound = original.SoundOnAttack.DefaultSound;
        SoundOnAttack.SoundType = original.SoundOnAttack.SoundType;
        SoundOnAttack.Volume = original.SoundOnAttack.Volume;
        SoundOnAttack.Pitch = original.SoundOnAttack.Pitch;

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
    }


    private void LateUpdate()
    {
        OnLateUpdate();
    }

    protected virtual void OnLateUpdate()
    {
        _wasDeflectedThisFrame = false;
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
        if (hitObject.TryGetComponent(out MeleeProjectile meleeProjectile) && meleeProjectile.DeflectCondition(this))
        {
            meleeProjectile.OnDeflect(this);
        }

        if (GameObjectUtility.TryGetComponentInSelfOrParent(hitObject.gameObject, out ObjectEffectsReceiver hitObjectEffectsReceiver))
        {
            hitObjectEffectsReceiver.ApplyEffect(HitEffects, this);
            if (WasDeflectedThisFrame) return;


            OnHitSomeOne?.Invoke(this, hitObject);
        }

        if (GameObjectUtility.TryGetComponentInSelfOrParent(hitObject.gameObject, out IDamagable hitDamagableObject))
        {
            hitDamagableObject.ApplyProjectileHit(this);
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
                    currentHitObjet.transform.parent.GetComponent<CharacterPart>() != null
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
