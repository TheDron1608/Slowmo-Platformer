using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeProjectile : AbstractProjectile
{
    const float DEFLECT_PARTICLE_VELOCITY = 2.5f;

    public float WallKnockback = 5f;
    public float BlockKnockback = 15f;
    public bool RepeatWallKnockback = false;
    public bool IsAbleToDeflectRangedProjectiles = true;
    public bool IsAbleToDeflectMeleeProjectiles = true;
    public List<AbstractEffect> EffectsOnDeflect = new();
    public List<AbstractEffect> SelfEffectsOnDeflect = new();
    public AbstractParticle ParticleOnDeflect = null;
    public AbstractSoundPlayer SoundOnDeflect;

    private bool _didHitAnyWallOnce = false;
    private Rigidbody2D _rigidBody;
    private int _hitWallLayerMask;
    private AbstractParticle _currentSpawnedParticle = null;

    public override Weapon Weapon
    {
        get => base.Weapon;
        protected set
        {
            base.Weapon = value;
            if (Weapon != null && Weapon.TryGetComponent(out Holdable holdableWeapon) && TryGetComponent(out DynamicMaterial dynamicMaterial))
            {
                dynamicMaterial.OverrideMaterial = holdableWeapon.EffectsReceiver.EffectMaterial;
            }
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _rigidBody)) throw new UnityException("RigidBody2D component not found");
    }

    protected override void SetAttrs(AbstractProjectile original, Quaternion direction, Vector2 position, ZIndexLayer layer, Weapon weapon)
    {
        base.SetAttrs(original, direction, position, layer, weapon);

        _hitWallLayerMask = 1 << layer.EnviromentLayer;

        MeleeProjectile meleeOriginal = original.GetComponent<MeleeProjectile>();

        WallKnockback = meleeOriginal.WallKnockback;
        BlockKnockback = meleeOriginal.BlockKnockback;
        RepeatWallKnockback = meleeOriginal.RepeatWallKnockback;
        IsAbleToDeflectRangedProjectiles = meleeOriginal.IsAbleToDeflectRangedProjectiles;
        IsAbleToDeflectMeleeProjectiles = meleeOriginal.IsAbleToDeflectMeleeProjectiles;
        EffectsOnDeflect = meleeOriginal.EffectsOnDeflect;
        SelfEffectsOnDeflect = meleeOriginal.SelfEffectsOnDeflect;
        ParticleOnDeflect = meleeOriginal.ParticleOnDeflect;

        SoundOnDeflect.DefaultSound = meleeOriginal.SoundOnDeflect.DefaultSound;
        SoundOnDeflect.SoundType = meleeOriginal.SoundOnDeflect.SoundType;
        SoundOnDeflect.Volume = meleeOriginal.SoundOnDeflect.Volume;
        SoundOnDeflect.Pitch = meleeOriginal.SoundOnDeflect.Pitch;

        _didHitAnyWallOnce = false;
    }

    private void FixedUpdate()
    {
        if (!IsAbleToHit) return;

        List<Collider2D> hitObjects = new();
        _rigidBody.Overlap(hitObjects);

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        for (int i = 0; i < hitObjects.Count; i++)
        {
            if (!IsAbleToHit) break;

            if (hitObjects[i].TryGetComponent(out AbstractProjectile deflectedProjectile) && DeflectCondition(deflectedProjectile))
            {
                OnDeflect(hitObjects[i].GetComponent<AbstractProjectile>());
            }
            else if (HitCondition(hitObjects, hitObjects[i]))
            {
                AddCurrentHittingCollidersItem(hitObjects[i]);
                OnHit(hitObjects[i].gameObject);
            }
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!Weapon.IsDestroyed())
        {
            transform.position = Weapon.ProjectileSpawnPosition.transform.position;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (RepeatWallKnockback && collision.gameObject.tag == LayerManager.ENVIROMENT_TAG_NAME && Weapon != null)
        {
            if (Weapon.TryGetComponent(out Holdable holdableWeapon) && holdableWeapon.CurrentHolder != null && holdableWeapon.CurrentHolder.TryGetComponent(out Rigidbody2D holderRigidBody))
            {
                holderRigidBody.linearVelocity -= VectorMath.Quartenion2DToVec2(transform.rotation) * WallKnockback * Time.deltaTime;
            }
            else if (Weapon.TryGetComponent(out Rigidbody2D weaponRigidBody) && weaponRigidBody.simulated)
            {
                weaponRigidBody.linearVelocity -= VectorMath.Quartenion2DToVec2(transform.rotation) * WallKnockback * Time.deltaTime;
            }
        }
    }

    public virtual bool DeflectCondition(AbstractProjectile deflected)
    {
        return
            (
                (deflected.GetComponent<MeleeProjectile>() != null && IsAbleToDeflectMeleeProjectiles) ||
                (deflected.GetComponent<RangedProjectile>() != null && IsAbleToDeflectRangedProjectiles)
            ) &&
            deflected != this &&
            (
                deflected.OwnerOrLastHolder == null ||
                (
                    deflected.OwnerOrLastHolder != Owner &&
                    (
                        FriendlyFire ||
                        deflected.FriendlyFire ||
                        (!OwnerOrLastHolder?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(deflected.OwnerOrLastHolder.CharComponents.CharacterTeam) ?? true)
                    )
                )
            );
    }

    public virtual void OnDeflect(AbstractProjectile defleclectedProjectile)
    {
        Vector3 deflectionPointPosition = (transform.position + defleclectedProjectile.transform.position) / 2;

        defleclectedProjectile.EffectsReceiver.ApplyEffect(EffectsOnDeflect, this);
        Owner?.CharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffectsOnDeflect, this, 1f, true);

        if (ParticleOnDeflect != null && (!_currentSpawnedParticle?.IsSpawned ?? true))
        {
            _currentSpawnedParticle = ParticleSpawner.SpawnParticle(
                ParticleOnDeflect,
                deflectionPointPosition,
                VectorMath.Quartenion2DToVec2(transform.rotation),
                0f,
                DEFLECT_PARTICLE_VELOCITY,
                0f,
                EffectsReceiver.EffectMaterial,
                LayerManager.Instance.GetZLayerOfGameObject(gameObject)
            );
        }
        SoundOnDeflect.PlaySound(false, deflectionPointPosition);
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
                Owner != charComponent.CharComponents.CharacterHolding
            ) &&
            (!GameObjectUtility.TryGetComponentInSelfOrParentOrChild(currentHitObjet.gameObject, out IDamagable damagableHitObject) || damagableHitObject.HitableByMeleeProjectiles) &&
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
                    hitObject.TryGetComponent(out MeleeProjectile meleeProjectile) &&
                    meleeProjectile != this &&
                    (meleeProjectile.Weapon == null || meleeProjectile.Weapon != Weapon) &&
                    meleeProjectile.IsAbleToDeflectMeleeProjectiles
                ) ||
                hitObjectBetween.collider.tag == LayerManager.ENVIROMENT_TAG_NAME
                )
            {
                return false;
            }
        }
        return true;
    }

    public override void RemoveProjectile()
    {
        base.RemoveProjectile();
        transform.parent = ProjectilesManager.Instance.UnusedMeleeProjectilesContainer;
    }
}
