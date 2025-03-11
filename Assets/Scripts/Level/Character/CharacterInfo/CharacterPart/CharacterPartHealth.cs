using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPartHealth : AbstractCharacterComponent
{
    public bool Cutable = false;
    public bool Gibable = false;
    public bool LosingLimbIsLethal = true;
    public float DamageMultiplier = 1.0f;
    public List<AbstractCharacterEffect> EffectsOnHit = new();

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    public void ApplyDamage(float damage, AbstractProjectile damager)
    {
        CharComponents.CharacterEffects.ApplyEffect(EffectsOnHit, damager, this);
        CharComponents.CharacterHealth.ApplyDamage(damage, damager, this);
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

        FluidParticleManager.Instance.SpawnFluidParticle(
            gameObject,
            FluidParticleManager.FluidParticlesSpreadTypes.GIB
            );
        GameObject.Destroy(gameObject);
    }
}
