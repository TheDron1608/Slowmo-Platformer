using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    public float Accuracy = 1f;
    public List<AbstractCharacterEffect> HitEffects = new();
    public List<AbstractCharacterEffect> SelfEffects = new();

    private Weapon _weapon;
    private CharacterHoldingObjects _owner;
    private Rigidbody2D _rigidBody;
    protected List<Collider2D> _currentHittingColliders = new();

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (!TryGetComponent(out _rigidBody)) throw new UnityException("RigidBody2D component not found");

        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        layer.UpdateLayerForGameObject(gameObject);
        transform.parent = layer.transform;
    }

    public Weapon Weapon
    {
        get => _weapon;
        protected set => _weapon = value;
    }
    public CharacterHoldingObjects Owner
    {
        get => _owner;
        protected set => _owner = value;
    }

    public List<AbstractProjectile> SpawnProjectile(Vector2 direction, float accuracityMultiplier = 1f, Weapon weapon = null)
    {
        return SpawnProjectile(VectorMath.Vec2ToQuarterninon2D(direction), accuracityMultiplier, weapon);
    }

    public List<AbstractProjectile> SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null)
    {
        List<AbstractProjectile> result = OnSpawnProjectile(direction, accuracityMultiplier, weapon);
        ApplySelfEffectOnWeaponUser(result, weapon);
        return result;
    }

    protected abstract List<AbstractProjectile> OnSpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null);

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
                holderCharComponents.CharacterEffects.ApplyEffect(SelfEffects, projectiles[i], null);
            }
        }
    }

    public virtual void OnHit(GameObject hitObject)
    {
        if (hitObject.transform.parent.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterEffects.ApplyEffect(HitEffects, this, hitObject.transform.parent.GetComponent<CharacterPartHealth>());
        }
    }

    public void RemoveSelf()
    {
        if (Weapon != null)
        {
            Weapon.Projectiles.Remove(this);
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
        List<Collider2D> hitObjects = new();

        _rigidBody.Overlap(hitObjects);

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        for (int i = 0; i < hitObjects.Count; i++)
        {
            if (HitCondition(hitObjects, hitObjects[i]))
            {
                _currentHittingColliders.Add(hitObjects[i]);
                OnHit(hitObjects[i].gameObject);
            }
        }
    }

    protected virtual bool HitCondition(List<Collider2D> totalHitObjects, Collider2D currentHitObjet)
    {
        return
            (
                !currentHitObjet.TryGetComponent(out AbstractCharacterComponent charComponent) ||
                charComponent.CharComponents.CharacterHolding.CurrentHoldObject == null ||
                (charComponent.CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon currentWeapon) && currentWeapon != Weapon)
            ) &&
            (
                !currentHitObjet.TryGetComponent(out CharacterHitbox charHitbox) ||
                (
                    charHitbox.HitableByProjectiles &&
                    GetIsHighestHitPriority(totalHitObjects, charHitbox)
                )
            ) &&
            !_currentHittingColliders.Contains(currentHitObjet) &&
            currentHitObjet.transform.parent.GetComponent<CharacterPart>() != null;
    }

    private bool GetIsHighestHitPriority(List<Collider2D> colliders, CharacterHitbox currentHitBox)
    {
        int currentHighestPriority = currentHitBox.HitPriority;
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].TryGetComponent(out CharacterHitbox charHitbox))
            {
                currentHighestPriority = Mathf.Max(currentHighestPriority, charHitbox.HitPriority);
            }
        }

        return currentHighestPriority <= currentHitBox.HitPriority;
    }
}
