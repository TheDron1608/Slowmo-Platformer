using System.Collections;
using UnityEngine;

public class CharacterHealth : AbstractCharacterComponent
{
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private float _minHealth = 0f;
    [SerializeField] private float _currentHealth = 10f;
    public float LivingWithDeadlyHealthSeconds = 0f;
    public bool CanHaveHealthOverMax = false;

    private bool _dead = false;
    private Coroutine _awaitLivingWithDeadlyHealthThenDieCoroutine;
    private AbstractProjectile _lastDamagedProjectile = null;
    private CharacterComponentsManager _lastDamagedAttacker = null;

    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            if (_currentHealth < _maxHealth)
            {
                Die();
            }
            else if (!Dead && _currentHealth > MinHealth)
            {
                StopDying();
            }
        }
    }

    public float MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            if (_currentHealth > _maxHealth && !CanHaveHealthOverMax)
            {
                CurrentHealth = _maxHealth;
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

    public bool Dead
    {
        get => _dead;
        private set => _dead = value;
    }

    public AbstractProjectile LastDamagedProjectile
    {
        get => _lastDamagedProjectile;
        private set => _lastDamagedProjectile = value;
    }
    public CharacterComponentsManager LastDamagedAttacker
    {
        get => _lastDamagedAttacker;
        private set => _lastDamagedAttacker = value;
    }

    public void Die()
    {
        if (LivingWithDeadlyHealthSeconds <= 0f)
        {
            InstantDie();
        }
        else
        {
            _awaitLivingWithDeadlyHealthThenDieCoroutine = StartCoroutine(AwaitLivingWithDeadlyHealthThenDie());
        }
    }

    public void StopDying()
    {
        if (_awaitLivingWithDeadlyHealthThenDieCoroutine != null)
        {
            StopCoroutine(_awaitLivingWithDeadlyHealthThenDieCoroutine);
        }
    }

    private IEnumerator AwaitLivingWithDeadlyHealthThenDie()
    {
        yield return new WaitForSeconds(LivingWithDeadlyHealthSeconds);
        InstantDie();
    }

    public void InstantDie()
    {
        Dead = true;
    }
}
