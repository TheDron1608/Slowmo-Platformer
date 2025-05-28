using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

public class MeleeProjectile : AbstractProjectile
{
    public float WallKnockback = 5f;
    public float BlockKnockback = 15f;
    public bool IsAbleTodeflectRangedProjectiles = true;
    public bool IsAbleTodeflectMeleeProjectiles = true;
    public List<AbstractEffect> EffectsOnDeflect = new();
    public List<AbstractEffect> SelfEffectsOnDeflect = new();

    private bool _didHitAnyWallOnce = false;
    private Rigidbody2D _rigidBody;
    private int _hitWallLayerMask;

    public override Weapon Weapon 
    { 
        get => base.Weapon;
        protected set
        {
            base.Weapon = value;
            if (Weapon != null && Weapon.TryGetComponent(out Holdable holdableWeapon) && TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sharedMaterial = holdableWeapon.EffectsReceiver.EffectMaterial;
                GetComponent<ObjectEffectsReceiver>()?.UpdateDefaultEffectMaterialToCurrent();
            }
        }
    }

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
                VectorMath.RandomizeQuarternion(direction, Accuracy),
                LayerManager.Instance.GetZLayerOfGameObject(weapon.gameObject).ProjectilesContainer
                );

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
        if (!IsAbleToHit) return;

        List<Collider2D> hitObjectsList = new();
        _rigidBody.Overlap(hitObjectsList);
        Collider2D[] hitObjects = hitObjectsList.ToArray();

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].TryGetComponent(out AbstractProjectile projectileHitObject) && projectileHitObject.OwnerOrLastHolder != Owner)
            {
                OnDeflect(projectileHitObject);
            }
            if (!IsAbleToHit) break;

            if (HitCondition(hitObjects, hitObjects[i]))
            {
                AddCurrentHittingCollidersItem(hitObjects[i]);
                OnHit(hitObjects[i].gameObject);
            }
            if (!IsAbleToHit) break;
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

    public virtual void OnDeflect(AbstractProjectile defleclectedProjectile)
    {
        defleclectedProjectile.OnDeflected(this);
        defleclectedProjectile.EffectsReceiver.ApplyEffect(EffectsOnDeflect, this);
        Owner?.CharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffectsOnDeflect, this);
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
            LayerManager.Instance.GetZLayerOfGameObject(gameObject).EntireLayerMask
            );

        foreach (RaycastHit2D hitObjectBetween in hitObjectsBetween)
        {
            if (hitObjectBetween.collider == hitObject)
            {
                return true;
            }
            if (
                (
                    hitObjectBetween.collider.GetComponent<MeleeProjectile>() != this && 
                    (hitObjectBetween.collider.GetComponent<MeleeProjectile>()?.IsAbleTodeflectMeleeProjectiles ?? false)
                ) ||
                hitObjectBetween.collider.tag == LayerManager.ENVIROMENT_TAG_NAME
                )
            {
                return false;
            }
        }
        return true;
    }
}
