using System;
using UnityEngine;

public class CharacterReloading : MonoBehaviour
{
    public bool IsAbleToReload = true;
    [SerializeField]
    private float _reloadSpeed = 1f;

    private CharacterHoldingObjects _characterHoldingObjectsComponent;

    public event EventHandler OnReload;

    public float ReloadSpeed
    {
        get => _reloadSpeed;
        set 
        {
            if (
                _characterHoldingObjectsComponent.CurrentHoldObject != null &&
                _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
            {
                rangedWeapon.SetReloadSpeed(value);
            }
            _reloadSpeed = value;
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _characterHoldingObjectsComponent)) throw new UnityException("CharacterHoldingObjects component not found");
    }

    public bool TryReload()
    {
        if (!IsAbleToReload) return false;

        if (
            _characterHoldingObjectsComponent.CurrentHoldObject != null &&
            _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
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
            _characterHoldingObjectsComponent.CurrentHoldObject != null &&
            _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
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
