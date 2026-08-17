using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputAttacking : AbstractAIAttacking
{
    public InputActionReference AttackActionReference;
    public InputActionReference AimActionReference;

    private bool _autoAttack = false;

    public bool AutoAttack
    {
        get => _autoAttack;
        set => _autoAttack = value;
    }

    private void OnEnable()
    {
        AttackActionReference.action.started += AttackActionRereference_OnActionStarted;
        AttackActionReference.action.canceled += AttackActionReference_OnActionCanceled;
    }

    private void AttackActionRereference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (UIManager.GamePaused()) return;
        HandleStartAttacking();
    }
    private void AttackActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleStopAttacking();
    }

    private void HandleStartAttacking()
    {
        CharComponents.CharacterAttacking.TryUseAttack(CharComponents.CharacterAiming.GetTargetAimNormalized());

        if (
            (
                CharComponents.CharacterHolding.CurrentHoldObject != null &&
                CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) &&
                weapon.AutoAttack
            ) ||
            (
                CharComponents.CharacterHolding.CurrentHoldObject == null &&
                CharComponents.UnarmedAttacking.AutoAttack
            )
            )
        {
            AutoAttack = true;
        }
    }

    private void HandleStopAttacking()
    {
        CharComponents.CharacterAttacking.TryStopShield();

        AutoAttack = false;
    }

    //AIM
    private void Update()
    {
        UpdateAimWeaponDown();
        UpdateAimInput();
        UpdateAutoAttack();
    }

    private void UpdateAimWeaponDown()
    {
        if (
            CharComponents.CharacterClumsyness.ClumsyRangedAttack &&
            CharComponents.CharacterMoving.GetCurrentMoveDirection() != 0f &&
            (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out RangedWeapon rw) ?? false)
            )
        {
            CharComponents.CharacterAiming.AimWeaponDown = true;
        }
        else
        {
            CharComponents.CharacterAiming.AimWeaponDown = false;
        }
    }

    private void UpdateAimInput()
    {
        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            CharComponents.CharacterAiming.TargetAimPoint = CharComponents.Center.transform.position + VectorMath.Vec2ToVec3(AimActionReference.action.ReadValue<Vector2>(), CharComponents.Center.transform.position.z);
        }
        else
        {
            Vector3? mousePos = CurrentDeviceTracker.GetMouseWorldPositionOnLayer(LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject));
            if (mousePos.HasValue)
            {
                CharComponents.CharacterAiming.TargetAimPoint = mousePos.Value;
            }
        }
    }

    private void UpdateAutoAttack()
    {
        if (
            AutoAttack &&
            (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out Weapon weapon) ?? false) &&
            CharComponents.CharacterAttacking != null
            )
        {
            CharComponents.CharacterAttacking.TryUseAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());

            if (weapon.TryGetComponent(out RangedWeapon rangedWeapon) && rangedWeapon.GetIsOutOfAmmo())
            {
                AutoAttack = false;
            }
        }
        else if (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) ?? false)
        {
            hammerWeapon.TrySetHammered(true);
        }
    }

    private void OnDisable()
    {
        AttackActionReference.action.started -= AttackActionRereference_OnActionStarted;
        AttackActionReference.action.canceled -= AttackActionReference_OnActionCanceled;
    }
}