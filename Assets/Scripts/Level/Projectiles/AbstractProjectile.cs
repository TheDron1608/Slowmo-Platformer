using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    const string ANIMATOR_RESET_TRIGGER_NAME = "Reset";

    public int AmountOnSpawn = 1;
    public float Accuracy = 1f;
    public List<AbstractEffect> HitEffects = new();
    public List<AbstractEffect> SelfEffects = new();
    public bool FriendlyFire = false;
    public bool IsAbleToHit = true;

    private Weapon _weapon = null;
    private Weapon _deflector = null;
    private CharacterHoldingObjects _owner = null;
    protected List<Collider2D> _currentHittingColliders = new();
    protected bool _wasDeflectedThisFrame = false;
    private ObjectEffectsReceiver _effectsReceiver;
    private BoxCollider2D _colliderComponent;

    public event EventHandler<GameObject> OnHitSomeOne;
    public event EventHandler OnDestroyed;

    public float ProjectileSize
    {
        get => _colliderComponent.size.x;
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
        protected set => _weapon = value;
    }
    public Weapon Deflector
    {
        get => _deflector;
        protected set => _deflector = value;
    }
    public CharacterHoldingObjects Owner
    {
        get => _owner;
        set => _owner = value;
    }
    public CharacterHoldingObjects OwnerOrLastHolder
    {
        get => Owner ?? Weapon?.GetComponent<Holdable>()?.LastHolder;
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

        ApplySelfEffectOnWeaponUser(result, weapon);

        return result;
    }

    protected virtual void SetAttrs(AbstractProjectile original, Quaternion direction, Vector2 position, ZIndexLayer layer, Weapon weapon)
    {
        Weapon = weapon;
        if (weapon?.TryGetComponent(out Holdable holdableWeapon) ?? false)
        {
            Owner = holdableWeapon.CurrentHolder ?? holdableWeapon.LastHolder;
        }
        transform.rotation = direction;
        transform.position = weapon.ProjectileSpawnPosition.transform.position;
        _deflector = null;
        gameObject.SetActive(true);

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

        _currentHittingColliders = new();
        _wasDeflectedThisFrame = false;

        LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);
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

    public virtual void OnDeflected(MeleeProjectile deflector)
    {
        if (_wasDeflectedThisFrame) return;

        Deflector = deflector.Weapon;
        _wasDeflectedThisFrame = true;
    }

    private void ApplySelfEffectOnWeaponUser(List<AbstractProjectile> projectiles, Weapon weapon)
    {
        if (
            weapon != null && 
            weapon.TryGetComponent(out Holdable holdableWeapon) &&
            holdableWeapon != null &&
            holdableWeapon.CurrentHolder != null &&
            holdableWeapon.CurrentHolder.TryGetComponent(out CharacterComponentsManager holderCharComponents)
            )
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                holderCharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffects, projectiles[i]);
            }
        }
    }

    public virtual void OnHit(GameObject hitObject)
    {
        if (hitObject.TryGetComponent(out MeleeProjectile meleeProjectile) && meleeProjectile.DeflectCondition(this))
        {
            meleeProjectile.OnDeflect(this);
        }

        ObjectEffectsReceiver hitObjectEffectsReceiver =
            hitObject.transform.GetComponent<ObjectEffectsReceiver>() ??
            hitObject.transform.GetComponentInParent<ObjectEffectsReceiver>();

        if (hitObjectEffectsReceiver != null)
        {
            hitObjectEffectsReceiver.ApplyEffect(HitEffects, this);
            OnHitSomeOne?.Invoke(this, hitObject);
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
                !currentHitObjet.TryGetComponent(out AbstractCharacterComponent charComponent) ||
                (
                    Owner == null ||
                    charComponent.CharComponents.CharacterHolding != Owner &&
                    (FriendlyFire || !charComponent.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(Owner.CharComponents.CharacterTeam))
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
}
