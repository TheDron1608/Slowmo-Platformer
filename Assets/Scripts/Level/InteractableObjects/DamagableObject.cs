using System;
using System.Collections.Generic;
using UnityEngine;

public class DamagableObject : MonoBehaviour, IDamagable
{
    const float PARTICLES_ON_DAMAGE_MIN_VELOCITY = 3f;
    const float PARTICLES_ON_DAMAGE_MAX_VELOCITY = 6f;
    const float PARTICLES_ON_DAMAGE_MIN_ANGULAR_VELOCITY = -360f;
    const float PARTICLES_ON_DAMAGE_MAX_ANGULAR_VELOCITY = 360f;
    const float PARTICLES_ON_DAMAGE_ACCURACY = 0.7f;

    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _minHealth = 0f;
    [SerializeField] private float _currentHealth = 10f;
    [SerializeField] private float _damageMultiplier = 1f;
    [SerializeField] private bool _piercableThrought = false;
    [SerializeField] private bool _hitableByMeleeProjectiles = true;
    [SerializeField] private bool _hitableByRangedProjectiles = true;
    public float LivingWithDeadlyHealthSeconds = 0f;
    public bool CanHaveHealthOverMax = false;
    public List<AbstractEffect> EffectsOnLethal = new();
    public List<AbstractParticle> ParticlesOnDamage = new();
    public AbstractSoundPlayer SoundOnDamage;

    private List<AbstractEffect> _defaultLethalEffects = null;
    protected bool _died = false;

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
        get => _hitableByMeleeProjectiles;
        set => _hitableByMeleeProjectiles = value;
    }

    public bool HitableByRangedProjectiles
    {
        get => _hitableByRangedProjectiles;
        set => _hitableByRangedProjectiles = value;
    }

    public float CurrentHealth
    {
        get => _currentHealth;
        protected set => _currentHealth = value;
    }

    public float MaxHealth
    {
        get => _maxHealth;
        protected set => _maxHealth = value;
    }

    public float MinHealth
    {
        get => _minHealth;
        protected set => _minHealth = value;
    }

    public List<AbstractEffect> DefaultEffectsOnLethal
    {
        get => _defaultLethalEffects;
        set => _defaultLethalEffects = value;  
    }

    public bool Died
    {
        get => _died;
    }

    private void Awake()
    {
        OnAwake();
    }
    protected virtual void OnAwake()
    {
        _defaultLethalEffects = EffectsOnLethal;
    }

    public void ApplyDamage(float damage, MonoBehaviour damager, float damageMultiplierMultiplier = 1f)
    {
        if (damager != null)
        {
            //spawning particles on hit
            RaycastHit2D hit = Physics2D.Raycast(
                damager.transform.position,
                VectorMath.Quartenion2DToVec2(damager.transform.rotation),
                Vector2.Distance(damager.transform.position, transform.position),
                1 << gameObject.layer
                );

            //chance to not spawn particle if damage is less than 1
            if (ParticlesOnDamage.Count > 0 && UnityEngine.Random.value < damage)
            {
                ParticleSpawner.SpawnInstantlyMultipleParticles(
                    ParticlesOnDamage,
                    hit.collider != null ? hit.point : GameObjectUtility.GetCenterOfCollider(GetComponent<Collider2D>()),
                    VectorMath.Quartenion2DToVec2(damager.transform.rotation),
                    0f,
                    PARTICLES_ON_DAMAGE_MIN_VELOCITY,
                    PARTICLES_ON_DAMAGE_MAX_VELOCITY,
                    PARTICLES_ON_DAMAGE_MIN_ANGULAR_VELOCITY,
                    PARTICLES_ON_DAMAGE_MAX_ANGULAR_VELOCITY,
                    GetComponent<ObjectEffectsReceiver>()?.EffectMaterial ?? GetComponent<SpriteRenderer>()?.material,
                    LayerManager.Instance.GetZLayerOfGameObject(gameObject),
                    Math.Max((int)damage, 1),
                    PARTICLES_ON_DAMAGE_ACCURACY
                    );
            }

            SoundOnDamage.PlaySound();
        }

        CurrentHealth -= damage;
        if (CurrentHealth >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        if (CurrentHealth <= MinHealth)
        {
            Die(damager);
        }
        if (CurrentHealth > MinHealth && !GetComponent<ObjectEffectsReceiver>().GetHasEffect<ILethalEffect>(false))
        {
            Ressurect();
        }
    }

    public void ApplyMaxHealth(float newMaxHealth, MonoBehaviour applier)
    {
        MaxHealth = newMaxHealth;
        if (CurrentHealth > MaxHealth && !CanHaveHealthOverMax)
        {
            CurrentHealth = MaxHealth;
        }
        if (CurrentHealth < MinHealth)
        {
            Die(applier);
        }
    }

    public void ApplyMinHealth(float newMinHealth, MonoBehaviour applier)
    {
        MinHealth = newMinHealth;
        if (CurrentHealth < MinHealth)
        {
            Die(applier);
        }
    }

    public virtual void Die(MonoBehaviour killer)
    {
        if (!_died && TryGetComponent(out ObjectEffectsReceiver effectsReceiver))
        {
            _died = true;
            effectsReceiver.ApplyEffect(EffectsOnLethal, killer);
        }
    }

    public virtual void Ressurect()
    {
        if (_died && TryGetComponent(out ObjectEffectsReceiver effectsReceiver))
        {
            _died = false;
            effectsReceiver.RemoveEffect(EffectsOnLethal);
            effectsReceiver.RemoveEffect<ILethalEffect>();
        }
    }

    public void ApplyProjectileHit(AbstractProjectile hitter)
    {
        OnHitByProjectile?.Invoke(this, hitter);
    }
}
