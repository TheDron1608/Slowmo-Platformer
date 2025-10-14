using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPartHealth : AbstractCharacterComponent, IDamagable
{
    const float BLEED_PARTICLES_ACCURACY = 0.85f;
    const float BLEED_PARTICLES_MIN_SPAWN_VELOCITY = 1f;
    const float BLEED_PARTICLES_MAX_SPAWN_VELOCITY = 4f;
    const float BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY = -180f;
    const float BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY = 180f;

    public bool Cutable = false;
    public bool Gibable = false;
    public bool LosingLimbIsLethal = true;
    public float DamageMultiplier = 1.0f;
    public List<AbstractParticle> ParticlesOnHit;
    public float ParticlesPerDamage = 0.5f;
    public int ParticlesAmountOnRemove = 15;
    public List<AbstractEffect> EffectsOnHit = new();
    [SerializeField] private bool _piercableThrought = false;
    [SerializeField] private bool _hitableByMeleeProjectiles = true;
    [SerializeField] private bool _hitableByRangedProjectiles = true;

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

    public void ApplyDamage(float damage, MonoBehaviour damager)
    {
        GetComponent<CharacterPart>().CharPartEffectsReceiver.ApplyEffect(EffectsOnHit, damager);
        CharComponents.CharacterHealth.ApplyDamage(damage, damager, GetComponent<CharacterPart>());


        if (ParticlesOnHit.Count > 0)
        {
            Vector3 hitPointPosition =
                damager.gameObject.transform.position +
                VectorMath.Quartenion2DToVec3(damager.transform.rotation) *
                Vector2.Distance(damager.gameObject.transform.position, transform.position);
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
                math.min(1, (int)math.floor(damage / ParticlesPerDamage)),
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
            if (limbParticle.TryGetComponent(out SpriteRenderer particleSprite) && TryGetComponent(out SpriteRenderer limbSprite))
            {
                particleSprite.sprite = null;
                GameObject newParticleRootSpriteContainer = new GameObject(name);
                newParticleRootSpriteContainer.transform.parent = limbParticle.transform;
                newParticleRootSpriteContainer.transform.localPosition = Vector3.zero;
                SpriteRenderer newParticleRootSprite = newParticleRootSpriteContainer.AddComponent<SpriteRenderer>();
                newParticleRootSprite.sprite = limbSprite.sprite;
                newParticleRootSprite.sortingOrder = limbSprite.sortingOrder;
                newParticleRootSprite.sharedMaterial = limbSprite.sharedMaterial;

                foreach (CharacterEquipmentPart equipment in GetComponent<CharacterPart>().GetEquipedAtParts())
                {
                    if (equipment.TryGetComponent(out SpriteRenderer equipmentSprite))
                    {
                        GameObject newParticleSpriteContainer = new GameObject(equipment.name);
                        newParticleSpriteContainer.transform.parent = limbParticle.transform;
                        newParticleSpriteContainer.transform.localPosition = Vector3.zero;
                        SpriteRenderer newParticleSprite = newParticleSpriteContainer.AddComponent<SpriteRenderer>();
                        newParticleSprite.sprite = equipmentSprite.sprite;
                        newParticleSprite.sortingOrder = limbSprite.sortingOrder + (equipmentSprite.sortingOrder % 10);
                        newParticleSprite.sharedMaterial = equipmentSprite.sharedMaterial;
                    }
                }
            }
        }

        if (ParticlesOnHit.Count > 0)
        {
            Vector3 cutPointPosition =
                cutter.gameObject.transform.position +
                VectorMath.Quartenion2DToVec3(cutter.transform.rotation) *
                Vector2.Distance(cutter.gameObject.transform.position, transform.position);
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnHit,
                cutPointPosition,
                VectorMath.Quartenion2DToVec2(cutter.transform.rotation),
                0f,
                BLEED_PARTICLES_MIN_SPAWN_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_VELOCITY,
                BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                CharComponents.CharacterEffectsReceiver.EffectMaterial,
                CharComponents.CharacterCollision.CurrentZLayer,
                ParticlesAmountOnRemove,
                BLEED_PARTICLES_ACCURACY
                );
        }

        if (TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.DestroyAllEquipments();
        }

        GameObject.Destroy(gameObject);
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

        if (ParticlesOnHit.Count > 0)
        {
            GameObjectUtility.TryGetComponentInSelfOrChild<Collider2D>(gameObject, out Collider2D collider);
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnHit,
                VectorMath.Vec3ToVec2(transform.position) + GameObjectUtility.GetCenterOfCollider(collider),
                Vector2.zero,
                0f,
                BLEED_PARTICLES_MIN_SPAWN_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_VELOCITY,
                BLEED_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                BLEED_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                CharComponents.CharacterEffectsReceiver.EffectMaterial,
                CharComponents.CharacterCollision.CurrentZLayer,
                ParticlesAmountOnRemove,
                0f
                );
        }

        if (TryGetComponent(out CharacterLimbPart limbPart))
        {
            limbPart.UnequipAllEquipments();
        }

        GameObject.Destroy(gameObject);
    }
}
