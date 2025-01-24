using UnityEngine;

public class CharacterAttacking : MonoBehaviour
{
    public bool IsAbleToAttack = true;
    public float AttackCooldownMultiplier = 1f;

    private bool _autoAttack = false;
    private Vector2 _autoAttackDirection = Vector2.right;

    private CharacterHoldingObjects _characterHoldingObjects;

    public bool AutoAttack
    {
        get => _autoAttack;
        set => _autoAttack = value;
    }

    public Vector2 AutoAttackDirection
    {
        get => _autoAttackDirection;
        set => _autoAttackDirection = value;
    }

    private void Awake()
    {
        _characterHoldingObjects = GetComponent<CharacterHoldingObjects>();
    }

    public bool TryHammerWeapon()
    {
        if (_characterHoldingObjects.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            return hammerWeapon.TrySetHammered(true);
        }
        return false;
    }

    public bool TryStopHammerringWeapon()
    {
        if (_characterHoldingObjects.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            return hammerWeapon.TrySetHammered(false);
        }
        return false;
    }

    public bool TryAttack(Vector2 direction)
    {
        if (_characterHoldingObjects.CurrentHoldObject != null && _characterHoldingObjects.CurrentHoldObject.TryGetComponent(out Weapon weapon))
        {
            return weapon.TryAttack(direction);
        }
        return false;
    }

    public bool TryHammerElseAttack(Vector2 direction)
    {
        if (_characterHoldingObjects.CurrentHoldObject != null)
        {
            if (_characterHoldingObjects.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
            {
                hammerWeapon.TrySetHammered(true);
                return true;
            }

            else if (_characterHoldingObjects.CurrentHoldObject.TryGetComponent(out Weapon weapon))
            {
                weapon.TryAttack(direction);
                return true;
            }
            return false;
        }

        return false;
    }

    private void FixedUpdate()
    {
        if (
            AutoAttack &&
            _characterHoldingObjects.CurrentHoldObject != null &&
            _characterHoldingObjects.CurrentHoldObject.TryGetComponent(out Weapon weapon) 
            )
        {

            if (weapon.IsAbleToAttack)
            {
                TryHammerElseAttack(_autoAttackDirection);
            }
            else if (weapon.TryGetComponent(out RangedWeapon rangedWeapon) && rangedWeapon.GetIsOutOfAmmo())
            {
                AutoAttack = false;
            }
        }
    }
}
