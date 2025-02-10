using System;
using UnityEngine;

public class CharacterReloading : AbstractCharacterComponent
{
    public bool IsAbleToReload = true;
    [SerializeField]
    private float _reloadSpeed = 1f;

    public event EventHandler OnReload;

    public float ReloadSpeed
    {
        get => _reloadSpeed;
        set 
        {
            if (
                _charComponents.CharacterHolding.CurrentHoldObject != null &&
                _charComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
            {
                rangedWeapon.SetReloadSpeed(value);
            }
            _reloadSpeed = value;
        }
    }

    public bool TryReload()
    {
        if (!IsAbleToReload) return false;

        if (
            _charComponents.CharacterHolding.CurrentHoldObject != null &&
            _charComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.TryReload()
            )
        {
            OnReload?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryUnload()
    {
        if (
            _charComponents.CharacterHolding.CurrentHoldObject != null &&
            _charComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.TryReload()
            )
        {
            OnReload?.Invoke(this, EventArgs.Empty);
            return true;
        }
        else
        {
            return false;
        }
    }
}
