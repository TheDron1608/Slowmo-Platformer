using System;
using System.Collections;
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
                CharComponents.CharacterHolding.CurrentHoldObject != null &&
                CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
            {
                rangedWeapon.SetReloadSpeed(value);
            }
            _reloadSpeed = value;
        }
    }

    public bool TryReload()
    {
        bool reloadResult = ForceReload();

        if (reloadResult)
        {
            if (CharComponents.CharacterClumsyness.ClumsyReloading)
            {
                StartCoroutine(AwaitFinishReloadThenStopAim());
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator AwaitFinishReloadThenStopAim()
    {
        CharComponents.CharacterMoving.ForceMove(0f);
        CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.CLUMSY_RELOAD;

        while (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.IsReloading
            )
        {
            yield return new WaitForFixedUpdate();
        }

        if (CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.CLUMSY_RELOAD)
        {
            CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.NONE;
        }
    }

    public bool ForceReload()
    {
        if (!IsAbleToReload) return false;
        if (CharComponents.CharacterClumsyness.ClumsyReloading && !CharComponents.CharacterCollision.IsCollidingFloor()) return false;

        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
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
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
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

    public bool TryFinishReload()
    {
        return
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.TryFinishReload();
    }

    public bool GetIsReloading()
    {
        return
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            rangedWeapon.IsReloading;
    }
}
