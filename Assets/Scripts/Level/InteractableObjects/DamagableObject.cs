using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DamagableObject : MonoBehaviour, IDamagable
{
    const float PARTICLES_ON_DAMAGE_MAX_VELOCITY = 6f;
    const float PARTICLES_ON_DAMAGE_MAX_ANGULAR_VELOCITY = 5f;

    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _minHealth = 0f;
    [SerializeField] private float _currentHealth = 10f;
    [SerializeField] private bool _piercableThrought = false;
    [SerializeField] private bool _hitableByMeleeProjectiles = true;
    [SerializeField] private bool _hitableByRangedProjectiles = true;
    public float LivingWithDeadlyHealthSeconds = 0f;
    public bool CanHaveHealthOverMax = false;
    public List<AbstractEffect> EffectsOnLethal = new();
    public List<GameObject> ParticlesOnDamage = new();

    public bool PiercableThrought 
    {
        get => _piercableThrought;
        set => _piercableThrought = value;
    }

    public bool HitableByMeleeProjectiles
    {
        get => _hitableByMeleeProjectiles;
        set => _hitableByMeleeProjectiles = value;
    }

    public bool HitableByRangedProjectiles
    {
        get => _hitableByRangedProjectiles;
        set => _hitableByRangedProjectiles = value;
    }

    private void Awake()
    {
        OnAwake();
    }
    protected virtual void OnAwake()
    {

    }

    public float CurrentHealth
    {
        get => _currentHealth;
        protected set => _currentHealth = value;
    }

    public float MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            if (_currentHealth > _maxHealth && !CanHaveHealthOverMax)
            {
                _currentHealth = _maxHealth;
            }
        }
    }

    public float MinHealth
    {
        get => _minHealth;
        set
        {
            _minHealth = value;
        }
    }

    public void ApplyDamage(float damage, MonoBehaviour damager)
    {
        //spawning particles on hit
        RaycastHit2D hit = Physics2D.Raycast(
            damager.transform.position,
            VectorMath.Quartenion2DToVec2(damager.transform.rotation),
            Vector2.Distance(damager.transform.position, transform.position),
            1 << gameObject.layer
            );

        for (int i = 0; i < (int)math.ceil(damage); i++)
        {
            if (UnityEngine.Random.value > damage) continue; //chance to not spawn particle if damage is less than 1

            GameObject newParticle = ParticleSpawner.SpawnParticle(
                NumberMath.PickRandomItemNoSeed(ParticlesOnDamage),
                transform,
                UnityEngine.Random.value * PARTICLES_ON_DAMAGE_MAX_VELOCITY,
                (UnityEngine.Random.value - 0.5f) * 2f * PARTICLES_ON_DAMAGE_MAX_ANGULAR_VELOCITY,
                0f,
                GetComponent<ObjectEffectsReceiver>()?.EffectMaterial ?? GetComponent<SpriteRenderer>()?.material,
                hit.collider != null ? hit.point : GameObjectUtility.GetCenterOfCollider(GetComponent<Collider2D>()),
                Quaternion.Inverse(damager.transform.rotation)
                );

            if (newParticle.TryGetComponent(out Collider2D newParticleCollider) && TryGetComponent(out Collider2D selfCollider))
            {
                Physics2D.IgnoreCollision(newParticleCollider, selfCollider);
            }
        }

        CurrentHealth -= damage;
        if (CurrentHealth <= MinHealth && ((!GetComponent<ObjectEffectsReceiver>()?.GetHasEffect<Death>()) ?? false))
        {
            Die(damager);
        }
    }

    public void Die(MonoBehaviour killer)
    {
        GetComponent<ObjectEffectsReceiver>()?.ApplyEffect(EffectsOnLethal, killer);
    }
}
