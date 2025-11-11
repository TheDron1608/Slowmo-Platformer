using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
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

    public Vector3? GetMouseWorldPositionOnCharacterLayer()
    {
        RaycastHit[] mouseHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        for (int i = 0; i < mouseHits.Length; i++)
        {
            if (mouseHits[i].collider.gameObject == LayerManager.Instance.GetZLayerOfGameObject(gameObject).gameObject)
            {
                return mouseHits[i].point;
            }
        }
        return null;
    }

    private void Start()
    {
        AttackActionReference.action.started += AttackActionRereference_OnActionStarted;
        AttackActionReference.action.canceled += AttackActionReference_OnActionCanceled;
    }

    private void AttackActionRereference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (GameplayUIManager.GamePaused()) return;
        HandleStartAttacking();
    }
    private void AttackActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleStopAttacking();
    }

    private void HandleStartAttacking()
    {
        CharComponents.CharacterAttacking.TryLoadElseAttack(CharComponents.CharacterAiming.GetTargetAimNormalized());

        if (
            (
                CharComponents.CharacterHolding.CurrentHoldObject != null &&
                CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) &&
                weapon.PlayerInputAutoAttackOnPress
            ) ||
            (
                CharComponents.CharacterHolding.CurrentHoldObject == null &&
                CharComponents.UnarmedAttacking.PlayerInputAutoAttackOnPress
            )
            )
        {
            AutoAttack = true;
        }
    }

    private void HandleStopAttacking()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon)
            )
        {
            CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
        }

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
            CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<RangedWeapon>() != null
            )
        {
            CharComponents.CharacterAiming.AimWeaponDown = true;
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
            Vector3? mousePos = GetMouseWorldPositionOnCharacterLayer();
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
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) &&
            CharComponents.CharacterAttacking != null
            )
        {
            CharComponents.CharacterAttacking.TryLoadElseAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());

            if (weapon.TryGetComponent(out RangedWeapon rangedWeapon) && rangedWeapon.GetIsOutOfAmmo())
            {
                AutoAttack = false;
            }
        }
    }

    private void OnDestroy()
    {
        AttackActionReference.action.started -= AttackActionRereference_OnActionStarted;
        AttackActionReference.action.canceled -= AttackActionReference_OnActionCanceled;
    }
}