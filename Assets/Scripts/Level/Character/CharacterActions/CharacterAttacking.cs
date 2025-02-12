using UnityEngine;

public class CharacterAttacking : AbstractCharacterComponent
{
    public bool IsAbleToAttack = true;
    public float AttackCooldownMultiplier = 1f;

    public bool TryHammerWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            return hammerWeapon.TrySetHammered(true);
        }
        return false;
    }

    public bool TryStopHammerringWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            return hammerWeapon.TrySetHammered(false);
        }
        return false;
    }

    public bool TryStartChainsaw()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Chainsaw chainsaw) && !chainsaw.Started)
        {
            return chainsaw.TryStart();
        }
        return false;
    }

    public bool TryAttack(Vector2 direction)
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon))
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
        if (CharComponents.CharacterHolding.CurrentHoldObject != null)
        {
            if (TryHammerWeapon()) return true;

            if (TryStartChainsaw()) return true;

            if (TryAttack(direction)) return true;

            return false;
        }

        return false;
    }
}
