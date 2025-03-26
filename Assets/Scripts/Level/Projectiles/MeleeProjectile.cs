using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeProjectile : AbstractProjectile
{
    public enum RangedProjectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        DEFLECT_PROJECTILE
    }
    public enum MeleerojectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        RESET_COOLDOWN,
        DISARM
    }

    public float WallKnockback = 5f;

    private bool _didHitAnyWallOnce = false;
    private Rigidbody2D _rigidBody;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _rigidBody)) throw new UnityException("RigidBody2D component not found");
    }

    protected override List<AbstractProjectile> OnSpawnProjectile(Quaternion direction, float accuracityMultiplier = 1, Weapon weapon = null)
    {

        MeleeProjectile newProjectile = Instantiate(
                this,
                weapon.transform.position,
                direction,
                LayerManager.Instance.GetZLayerOfGameObject(weapon.gameObject).transform
                );

        newProjectile.transform.position = weapon.transform.position;
        newProjectile.transform.rotation = VectorMath.RandomizeQuarternion(direction, Accuracy);

        newProjectile.Weapon = weapon;
        if (weapon != null && weapon.TryGetComponent(out Holdable holdableWeapon))
        {
            newProjectile.Owner = holdableWeapon.CurrentHolder;
        }

        return new List<AbstractProjectile>() { newProjectile };
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        List<Collider2D> hitObjectsList = new();
        _rigidBody.Overlap(hitObjectsList);
        Collider2D[] hitObjects = hitObjectsList.ToArray();

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (HitCondition(hitObjects, hitObjects[i]))
            {
                _currentHittingColliders.Add(hitObjects[i]);
                OnHit(hitObjects[i].gameObject);
            }
        }

        if (!Weapon.IsDestroyed())
        {
            transform.position = Weapon.transform.position;
        }
    }

    public override void OnHit(GameObject hitObject)
    {
        base.OnHit(hitObject);

        if (_didHitAnyWallOnce) return;

        if (hitObject.tag == LayerManager.ENVIROMENT_TAG_NAME && Weapon != null) 
        {
            if (Weapon.TryGetComponent(out Holdable holdableWeapon) && holdableWeapon.CurrentHolder != null && holdableWeapon.CurrentHolder.TryGetComponent(out Rigidbody2D holderRigidBody))
            {
                holderRigidBody.linearVelocity -= VectorMath.Quartenion2DToVec2(transform.rotation) * WallKnockback;
            }
            else if (Weapon.TryGetComponent(out Rigidbody2D weaponRigidBody) && weaponRigidBody.simulated)
            {
                weaponRigidBody.linearVelocity -= VectorMath.Quartenion2DToVec2(transform.rotation) * WallKnockback;
            }
            _didHitAnyWallOnce = true;
        }
    }

    protected override bool HitCondition(Collider2D[] totalHitObjects, Collider2D currentHitObjet)
    {
        return 
            base.HitCondition(totalHitObjects, currentHitObjet) &&
            (
                !currentHitObjet.TryGetComponent(out AbstractCharacterComponent charComponent) ||
                charComponent.CharComponents.CharacterHolding.LastHoldObject == null ||
                (charComponent.CharComponents.CharacterHolding.LastHoldObject.TryGetComponent(out Weapon lastWeapon) && lastWeapon != Weapon
            )
        );
    }
}
