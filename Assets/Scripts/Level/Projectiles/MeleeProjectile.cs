using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

public class MeleeProjectile : AbstractProjectile
{
    public enum RangedProjectileDeflectionType
    {
        NO_DEFLECT,
        ABSORB_PROJECTILE,
        DEFLECT_PROJECTILE,
        DEFLECT_PROJECTILE_TO_ENEMY
    }
    public enum MeleeProjectileDeflectionType
    {
        NO_DEFLECT,
        BLOCK,
        DISARM
    }

    public float WallKnockback = 5f;
    public float BlockKnockback = 15f;
    public RangedProjectileDeflectionType RangedProjectileDeflection = RangedProjectileDeflectionType.DEFLECT_PROJECTILE;
    public MeleeProjectileDeflectionType MeleeProjectileDeflection = MeleeProjectileDeflectionType.BLOCK;

    private bool _didHitAnyWallOnce = false;
    private Rigidbody2D _rigidBody;
    private int _hitWallLayerMask;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _rigidBody)) throw new UnityException("RigidBody2D component not found");

        _hitWallLayerMask = 1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EnviromentLayer;
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
        else if (weapon != null && weapon.TryGetComponent(out UnarmedWeapon unarmedWeapon))
        {
            newProjectile.Owner = unarmedWeapon.CharComponents.CharacterHolding;
        }

        return new List<AbstractProjectile>() { newProjectile };
    }

    private void FixedUpdate()
    {
        List<Collider2D> hitObjectsList = new();
        _rigidBody.Overlap(hitObjectsList);
        Collider2D[] hitObjects = hitObjectsList.ToArray();

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].TryGetComponent(out AbstractProjectile projectileHitObject) && projectileHitObject.Owner != Owner)
            {
                projectileHitObject.OnDeflected(this);
                OnDeflect(projectileHitObject);
            }
            if (HitCondition(hitObjects, hitObjects[i]))
            {
                _currentHittingColliders.Add(hitObjects[i]);
                OnHit(hitObjects[i].gameObject);
            }
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!Weapon.IsDestroyed())
        {
            transform.position = Weapon.transform.position;
        }
    }

    public override void OnDeflected(MeleeProjectile deflector)
    {
        switch (deflector.MeleeProjectileDeflection)
        {
            case MeleeProjectileDeflectionType.NO_DEFLECT:
                break;

            case MeleeProjectileDeflectionType.BLOCK:
                if (deflector.Owner != null && deflector.Weapon.GetComponent<Holdable>() == deflector.Owner.CurrentHoldObject)
                {
                    deflector.Owner.CharComponents.CharacterRigidBody.linearVelocity = VectorMath.Quartenion2DToVec2(transform.rotation) * deflector.BlockKnockback;
                    if (deflector.Owner.CharComponents.CharacterCollision.IsCollidingFloor())
                    {
                        deflector.Owner.CharComponents.CharacterRigidBody.linearVelocityY = math.max(3f, deflector.Owner.CharComponents.CharacterRigidBody.linearVelocityY);
                    }
                }
                break;

            case MeleeProjectileDeflectionType.DISARM:
                if (Owner != null && Weapon.GetComponent<Holdable>() == Owner.CurrentHoldObject)
                {
                    Owner.CharComponents.CharacterRigidBody.linearVelocity = VectorMath.Quartenion2DToVec2(deflector.transform.rotation) * BlockKnockback;
                    if (Owner.CharComponents.CharacterCollision.IsCollidingFloor())
                    {
                        Owner.CharComponents.CharacterRigidBody.linearVelocityY = math.max(3f, Owner.CharComponents.CharacterRigidBody.linearVelocityY);
                    }

                    Owner.CharComponents.CharacterHolding.TryThrow(Owner.CharComponents.CharacterRigidBody.linearVelocity.normalized, 0.5f);
                }
                break;
        }
    }

    protected virtual void OnDeflect(AbstractProjectile defleclectedProjectile)
    {
        switch (MeleeProjectileDeflection)
        {
            case MeleeProjectileDeflectionType.NO_DEFLECT:
                break;

            case MeleeProjectileDeflectionType.BLOCK:
                if (Owner != null && Weapon.GetComponent<Holdable>() == Owner.CurrentHoldObject)
                {
                    Owner.CharComponents.CharacterRigidBody.linearVelocity = VectorMath.Quartenion2DToVec2(defleclectedProjectile.transform.rotation) * BlockKnockback;
                    if (Owner.CharComponents.CharacterCollision.IsCollidingFloor())
                    {
                        Owner.CharComponents.CharacterRigidBody.linearVelocityY = math.max(3f, Owner.CharComponents.CharacterRigidBody.linearVelocityY);
                    }
                }
                break;
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
                Owner != charComponent.CharComponents.CharacterHolding
            ) &&
            GetHasNoBlocksBetweenHitObject(currentHitObjet);
    }

    private bool GetHasNoBlocksBetweenHitObject(Collider2D hitObject)
    {
        RaycastHit2D[] hitObjectsBetween = Physics2D.LinecastAll(
            transform.position, 
            hitObject.transform.position,
            1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EntireLayerMask
            );
        foreach (RaycastHit2D hitObjectBetween in hitObjectsBetween)
        {
            if (hitObjectBetween.collider == hitObject)
            {
                return true;
            }
            if (
                hitObjectBetween.collider.GetComponent<MeleeProjectile>()?.MeleeProjectileDeflection != MeleeProjectileDeflectionType.NO_DEFLECT ||
                hitObjectBetween.collider.tag == LayerManager.ENVIROMENT_TAG_NAME
                )
            {
                return false;
            }
        }
        return true;
    }
}
