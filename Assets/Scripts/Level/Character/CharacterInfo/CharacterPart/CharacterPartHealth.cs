using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPartHealth : AbstractCharacterComponent
{
    public bool Cutable = false;
    public bool Gibable = false;
    public bool CanBleed = false;
    public bool LosingLimbIsLethal = true;
    public float DamageMultiplier = 1.0f;
    public List<AbstractCharacterEffect> EffectsOnHit = new();

    public void ApplyDamage(float damage, MonoBehaviour damager)
    {
        CharComponents.CharacterEffects.ApplyEffect(EffectsOnHit, damager, this);
        CharComponents.CharacterHealth.ApplyDamage(damage, damager, this);

        Vector3 hitPointPosition =
            damager.gameObject.transform.position +
            VectorMath.Quartenion2DToVec3(damager.transform.rotation) *
            Vector2.Distance(damager.gameObject.transform.position, transform.position);

        if (CanBleed)
        {
            FluidParticleManager.Instance.SpawnFluidParticles(
                hitPointPosition,
                transform,
                LayerManager.Instance.GetZLayerOfGameObject(gameObject),
                (CharComponents.CharacterEffects.TryGetEffect(out Death death) && death.DiedThisFrame) ? 
                    FluidParticleManager.FluidParticlesSpreadTypes.LETHAL : 
                    FluidParticleManager.FluidParticlesSpreadTypes.DAMAGE,
                damager.transform.rotation
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
            CharComponents.CharacterHealth.Die(cutter, this);
        }

        if (TryGetComponent(out ParticleSpawner limbParticleSpawner))
        {
            if (CharComponents.CharacterRigidBody.linearVelocityX < 0f)
            {
                limbParticleSpawner.SpawnAngle = limbParticleSpawner.SpawnAngle + (90f - limbParticleSpawner.SpawnAngle) * 2;
                limbParticleSpawner.SpawnAngularVeclocity *= -1f;
            }
            limbParticleSpawner.SpawnParticle();
        }

        if (CanBleed)
        {
            FluidParticleManager.Instance.SpawnFluidParticles(
                gameObject,
                FluidParticleManager.FluidParticlesSpreadTypes.LETHAL,
                cutter.transform.rotation
                );
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
            CharComponents.CharacterHealth.Die(gibber, this);
        }

        if (CanBleed)
        {
            FluidParticleManager.Instance.SpawnFluidParticles(
                gameObject,
                FluidParticleManager.FluidParticlesSpreadTypes.HEADSHOT,
                gibber.transform.rotation
                );
        }

        GameObject.Destroy(gameObject);
    }
}
