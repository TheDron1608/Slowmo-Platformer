using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHealth : AbstractCharacterComponent
{
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _minHealth = 0f;
    [SerializeField] private float _currentHealth = 10f;
    public float LivingWithDeadlyHealthSeconds = 0f;
    public bool CanHaveHealthOverMax = false;
    public List<AbstractCharacterEffect> EffectsOnLethal = new();

    public float CurrentHealth
    {
        get => _currentHealth;
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

    public void ApplyDamage(float damage, MonoBehaviour damager, CharacterPartHealth damagedPart)
    {
        _currentHealth -= damage;
        if (_currentHealth <= MinHealth && !CharComponents.CharacterEffects.GetHasEffect<Death>())
        {
            Die(damager, damagedPart);
        }
    }

    public void Die(MonoBehaviour killer, CharacterPartHealth lethallyDamagedPart)
    {
        CharComponents.CharacterEffects.ApplyEffect(EffectsOnLethal, killer, lethallyDamagedPart);
    }
}
