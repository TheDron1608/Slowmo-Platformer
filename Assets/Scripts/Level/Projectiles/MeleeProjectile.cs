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
        transform.position = Weapon.transform.position;
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

    protected override bool HitCondition(List<Collider2D> totalHitObjects, Collider2D currentHitObjet)
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
