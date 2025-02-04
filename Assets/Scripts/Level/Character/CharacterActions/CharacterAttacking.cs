using UnityEngine;

public class CharacterAttacking : MonoBehaviour
{
    public bool IsAbleToAttack = true;
    public float AttackCooldownMultiplier = 1f;

    private CharacterHoldingObjects _characterHoldingObjects;

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

    public bool TryStartChainsaw()
    {
        if (_characterHoldingObjects.CurrentHoldObject.TryGetComponent(out Chainsaw chainsaw) && !chainsaw.Started)
        {
            return chainsaw.TryStart();
        }
        return false;
    }

    public bool TryAttack(Vector2 direction)
    {
        if (_characterHoldingObjects.CurrentHoldObject != null && _characterHoldingObjects.CurrentHoldObject.TryGetComponent(out Weapon weapon))
        {
            if (weapon.TryAttack(direction))
            {
                if (TryGetComponent(out CharacterRolling charRolling))
                {
                    charRolling.ForceStopRolling();
                }
                return true;
            }
        }
        return false;
    }

    public bool TryLoadElseAttack(Vector2 direction)
    {
        if (_characterHoldingObjects.CurrentHoldObject != null)
        {
            if (TryHammerWeapon()) return true;

            if (TryStartChainsaw()) return true;

            if (TryAttack(direction)) return true;

            return false;
        }

        return false;
    }
}
