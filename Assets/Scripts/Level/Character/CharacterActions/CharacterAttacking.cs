using UnityEngine;

public class CharacterAttacking : AbstractCharacterComponent
{
    [SerializeField] private bool _isAbleToAttack = true;
    [SerializeField] private bool _isAbleToHammer = true;
    [SerializeField] private bool _isAbleToStartChainsaw = true;
    public float AttackCooldownMultiplier = 1f;

    public bool IsAbleToAttack
    {
        get => _isAbleToAttack;
        set => _isAbleToAttack = value;
    }
    public bool IsAbleToHammer
    {
        get => _isAbleToHammer;
        set
        {
            if (!value)
            {
                TryStopHammerringWeapon();
            }
            _isAbleToHammer = value;
        }
    }
    public bool IsAbleToStartChainsaw
    {
        get => _isAbleToStartChainsaw;
        set => _isAbleToStartChainsaw = value;
    }

    public bool TryHammerWeapon()
    {
        if (IsAbleToHammer && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            return hammerWeapon.TrySetHammered(true);
        }
        return false;
    }

    public bool TryStopHammerringWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            return hammerWeapon.TrySetHammered(false);
        }
        return false;
    }

    public bool TryStartChainsaw()
    {
        if (IsAbleToStartChainsaw && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Chainsaw chainsaw) && !chainsaw.Started)
        {
            return chainsaw.TryStart();
        }
        return false;
    }

    public bool TryAttack(Vector2 direction)
    {
        if (IsAbleToAttack && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon))
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
