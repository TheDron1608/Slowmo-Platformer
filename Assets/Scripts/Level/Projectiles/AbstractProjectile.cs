using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractProjectile : MonoBehaviour
{
    public float Accuracy = 1f;
    public float KnockBack = 2.5f;
    public float SelfKnockBack = 0f;
    public List<AbstractCharacterEffect> EffectsOnHit = new();

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
        _rigidBody = GetComponent<Rigidbody2D>();
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

    public abstract List<AbstractProjectile> SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null);

    public virtual void OnHit(GameObject hitObject)
    {
        if (hitObject.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            charComponent.CharComponents.CharacterEffects.ApplyEffect(EffectsOnHit);
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
            if (
                (
                    !hitObjects[i].TryGetComponent(out AbstractCharacterComponent charComponent) ||
                    charComponent.CharComponents.CharacterHolding.CurrentHoldObject == null ||
                    (charComponent.CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) && weapon != Weapon)
                ) &&
                (
                    !hitObjects[i].TryGetComponent(out CharacterHitbox charHitbox) ||
                    (
                        charHitbox.HitableByProjectiles &&
                        GetIsHighestHitPriority(hitObjects, charHitbox)
                    )
                ) &&
                !_currentHittingColliders.Contains(hitObjects[i])
            )
            {
                _currentHittingColliders.Add(hitObjects[i]);
                OnHit(hitObjects[i].gameObject);
                if (hitObjects[i].TryGetComponent(out CharacterHitbox characterHitbox))
                {
                    characterHitbox.OnHit(this);
                }
            }
        }
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
