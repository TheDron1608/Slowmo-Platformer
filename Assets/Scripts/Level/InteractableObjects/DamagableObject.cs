using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagableObject : MonoBehaviour, IDamagable
{
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _minHealth = 0f;
    [SerializeField] private float _currentHealth = 10f;
    [SerializeField] private bool _piercableThrought = false;
    public float LivingWithDeadlyHealthSeconds = 0f;
    public bool CanHaveHealthOverMax = false;
    public List<AbstractEffect> EffectsOnLethal = new();

    public bool PiercableThrought 
    {
        get => _piercableThrought;
        set => _piercableThrought = value;
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
