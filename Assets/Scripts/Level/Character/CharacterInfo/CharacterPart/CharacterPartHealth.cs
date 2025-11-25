using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPartHealth : AbstractCharacterComponent, IDamagable
{
    const float BLEED_PARTICLES_ACCURACY = 0.66f;
    const float BLEED_PARTICLES_MIN_SPAWN_VELOCITY = 2f;
    const float BLEED_PARTICLES_MAX_SPAWN_VELOCITY = 6f;
    const float BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY = -180f;
    const float BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY = 180f;

    public bool Cutable = false;
    public bool Gibable = false;
    public bool LosingLimbIsLethal = true;
    public List<AbstractParticle> ParticlesOnHit = new();
    public List<AbstractParticle> ParticlesOnGib = new();
    public List<AbstractParticle> ParticlesOnCutOff = new();
    public List<AbstractParticle> ParticlesOnLethalHit = new();
    public float ParticlesPerDamage = 1.5f;
    public int ParticlesAmountOnLethal = 15;
    public List<AbstractEffect> EffectsOnHit = new();
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private bool _piercableThrought = false;
    [SerializeField] private bool _hitableByMeleeProjectiles = true;
    [SerializeField] private bool _hitableByRangedProjectiles = true;

    public event EventHandler<AbstractProjectile> OnHitByProjectile;

    public float DamageMultiplier
    {
        get => _damageMultiplier;
        set => _damageMultiplier = value;
    }

    public bool PiercableThrought
    {
        get => _piercableThrought;
        set => _piercableThrought = value;
    }

    public bool HitableByMeleeProjectiles
    {
        get => _hitableByMeleeProjectiles && CharComponents.CharacterHealth.HitableByMeleeProjectiles;
        set => _hitableByMeleeProjectiles = value;
    }

    public bool HitableByRangedProjectiles
    {
        get => _hitableByRangedProjectiles && CharComponents.CharacterHealth.HitableByRangedProjectiles;
        set => _hitableByRangedProjectiles = value;
    }

    public void ApplyDamage(float damage, MonoBehaviour damager, float damageMultiplierMultplier = 1f)
    {
        GetComponent<CharacterPart>().CharPartEffectsReceiver.ApplyEffect(EffectsOnHit, damager);
        CharComponents.CharacterHealth.ApplyDamage(math.lerp(damage, damage * DamageMultiplier, damageMultiplierMultplier), damager, gameObject.GetComponent<CharacterPart>());

        Vector3 hitPointPosition =
            damager.transform.position +
            VectorMath.Quartenion2DToVec3(damager.transform.rotation) *
            Vector2.Distance(damager.transform.position, CharComponents.Center.transform.position);

        if (ParticlesOnHit.Count > 0)
        {
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnHit,
                hitPointPosition,
                VectorMath.Quartenion2DToVec2(damager.transform.rotation),
                0f,
                BLEED_PARTICLES_MIN_SPAWN_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_VELOCITY,
                BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                CharComponents.CharacterEffectsReceiver.EffectMaterial,
                CharComponents.CharacterCollision.CurrentZLayer,
                math.max(1, (int)math.floor(damage * ParticlesPerDamage)),
                BLEED_PARTICLES_ACCURACY
                );
        }

        if (CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() && ParticlesOnLethalHit.Count > 0)
        {
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnLethalHit,
                hitPointPosition,
                VectorMath.Quartenion2DToVec2(damager.transform.rotation),
                0f,
                BLEED_PARTICLES_MIN_SPAWN_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_VELOCITY,
                BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                CharComponents.CharacterEffectsReceiver.EffectMaterial,
                CharComponents.CharacterCollision.CurrentZLayer,
                ParticlesAmountOnLethal,
                BLEED_PARTICLES_ACCURACY
                );
        }
    }

    public bool TryCutOff(MonoBehaviour cutter)
    {
        if (Cutable)
        {
            CutOff(cutter);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CutOff(MonoBehaviour cutter)
    {
        if (LosingLimbIsLethal)
        {
            CharComponents.CharacterHealth.Die(cutter, GetComponent<CharacterPart>());
        }

        SpawnCutLimbParticle();

        if (ParticlesOnCutOff.Count > 0)
        {
            Vector3 cutPointPosition =
                cutter.gameObject.transform.position +
                VectorMath.Quartenion2DToVec3(cutter.transform.rotation) *
                Vector2.Distance(cutter.gameObject.transform.position, transform.position);
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnCutOff,
                cutPointPosition,
                VectorMath.Quartenion2DToVec2(cutter.transform.rotation),
                0f,
                BLEED_PARTICLES_MIN_SPAWN_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_VELOCITY,
                BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                CharComponents.CharacterEffectsReceiver.EffectMaterial,
                CharComponents.CharacterCollision.CurrentZLayer,
                ParticlesAmountOnLethal,
                BLEED_PARTICLES_ACCURACY
                );
        }

        if (TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.DestroyAllEquipments();
        }

        GameObject.Destroy(gameObject);
    }

    private void SpawnCutLimbParticle()
    {
        if (TryGetComponent(out ParticleSpawner limbParticleSpawner))
        {
            if (CharComponents.CharacterRigidBody.linearVelocityX < 0f)
            {
                limbParticleSpawner.SpawnAngle = limbParticleSpawner.SpawnAngle + (90f - limbParticleSpawner.SpawnAngle) * 2;
                limbParticleSpawner.SpawnAngularVeclocity *= -1f;
            }
            AbstractParticle limbParticle = limbParticleSpawner.SpawnParticle();

            if (limbParticle.TryGetComponent(out BoxCollider2D particleCollider) && (GetComponent<CharacterLimbPart>()?.CharPartHitbox.TryGetComponent(out Collider2D limbCollider) ?? false))
            {
                GameObjectUtility.ConvertSimpleColliderToBoxCollider(particleCollider, limbCollider);
            }
            if (limbParticle.TryGetComponent(out SpriteRenderer particleSprite) && TryGetComponent(out SpriteRenderer limbParticleSprite))
            {
                particleSprite.sprite = null;
                GameObject newParticleRootSpriteContainer = new GameObject(name);
                newParticleRootSpriteContainer.transform.parent = limbParticle.transform;
                newParticleRootSpriteContainer.transform.localPosition = Vector3.zero;
                newParticleRootSpriteContainer.transform.rotation = limbParticle.transform.rotation;
                SpriteRenderer newParticleRootSprite = newParticleRootSpriteContainer.AddComponent<SpriteRenderer>();
                newParticleRootSprite.sprite = limbParticleSprite.sprite;
                newParticleRootSprite.sortingOrder = limbParticleSprite.sortingOrder;
                newParticleRootSprite.sortingLayerID = particleSprite.sortingLayerID;
                newParticleRootSprite.sharedMaterial = limbParticleSprite.sharedMaterial;

                foreach (CharacterEquipmentPart equipment in GetComponent<CharacterPart>().GetEquipedAtParts())
                {
                    if (equipment.TryGetComponent(out SpriteRenderer equipmentSprite))
                    {
                        GameObject newParticleSpriteContainer = new GameObject(equipment.name);
                        newParticleSpriteContainer.transform.parent = limbParticle.transform;
                        newParticleSpriteContainer.transform.localPosition = Vector3.zero;
                        newParticleSpriteContainer.transform.rotation = limbParticle.transform.rotation;
                        SpriteRenderer newParticleSprite = newParticleSpriteContainer.AddComponent<SpriteRenderer>();
                        newParticleSprite.sprite = equipmentSprite.sprite;
                        newParticleSprite.sortingOrder = limbParticleSprite.sortingOrder + (equipmentSprite.sortingOrder % 10);
                        newParticleSprite.sortingLayerID = particleSprite.sortingLayerID;
                        newParticleSprite.sharedMaterial = equipmentSprite.sharedMaterial;
                    }
                }
            }
        }
    }

    public bool TryGib(MonoBehaviour gibber)
    {
        if (Gibable)
        {
            Gib(gibber);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Gib(MonoBehaviour gibber)
    {
        if (LosingLimbIsLethal)
        {
            CharComponents.CharacterHealth.Die(gibber, GetComponent<CharacterPart>());
        }

        if (ParticlesOnGib.Count > 0)
        {
            GameObjectUtility.TryGetComponentInSelfOrChild<Collider2D>(gameObject, out Collider2D collider);
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnGib,
                GameObjectUtility.GetCenterOfCollider(collider),
                Vector2.zero,
                0f,
                BLEED_PARTICLES_MIN_SPAWN_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_VELOCITY,
                BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                CharComponents.CharacterEffectsReceiver.EffectMaterial,
                CharComponents.CharacterCollision.CurrentZLayer,
                ParticlesAmountOnLethal,
                0f
                );
        }

        if (TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.UnequipAllEquipments();
        }

        GameObject.Destroy(gameObject);
    }

    public void ApplyProjectileHit(AbstractProjectile hitter)
    {
        OnHitByProjectile?.Invoke(this, hitter);
        CharComponents.CharacterHealth.ApplyProjectileHit(hitter);
    }
}
