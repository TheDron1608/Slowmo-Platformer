using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[Serializable]
public class CharacterPlayerInputHandler : AbstractCharacterComponent
{
    public InputActionReference MoveActionReference;
    public InputActionReference JumpActionReference;
    public InputActionReference AimActionReference;
    public InputActionReference AttackActionReference;
    public InputActionReference InteractActionReference;
    public InputActionReference GrabActionReference;
    public InputActionReference ReloadActionReference;
    public float MinMoveSpeed = 0.5f;

    private Coroutine MoveGamepadActionHandler;

    public float CoyoteEarlyTimer = .33f;
    public float CoyoteLateTimer = .125f;
    public float GamePadRollInputDelay = 0.075f;

    private float _coyoteJumpTooEarlyTimeLeft = 0f;
    private Coroutine _coyoteJumpTooEarlyHandler;
    private Holdable _currentSelectedGrabObject = null;
    private Interactable _currentSelectedInteractObject = null;
    private bool _autoAttack = false;
    private bool _awaitingResetInputToReroll = false;
    private float _currentGamepadRollInputDelay = 0f;

    public bool AutoAttack
    {
        get => _autoAttack;
        set => _autoAttack = value;
    }

    public Holdable CurrentSelectedGrabObject
    {
        get => _currentSelectedGrabObject;
        private set
        {
            if (value != _currentSelectedGrabObject)
            {
                if (_currentSelectedGrabObject != null)
                {
                    _currentSelectedGrabObject.Selected = false;
                }
                if (value != null)
                {
                    value.Selected = true;
                }
            }
            _currentSelectedGrabObject = value;
        }
    }

    public Interactable CurrentSelectedInteractObject
    {
        get => _currentSelectedInteractObject;
        private set
        {
            if (value != _currentSelectedInteractObject)
            {
                if (_currentSelectedInteractObject != null)
                {
                    _currentSelectedInteractObject.Selected = false;
                }
                if (value != null)
                {
                    value.Selected = true;
                }
            }
            _currentSelectedInteractObject = value;
        }
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
        JumpActionReference.action.started += JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled += JumpActionReference_OnActionCanceled;
        InteractActionReference.action.started += InteractActionReference_OnActionStarted;
        GrabActionReference.action.started += GrabActionReference_OnActionStarted;
        AttackActionReference.action.started += AttackActionRereference_OnActionStarted;
        AttackActionReference.action.canceled += AttackActionReference_OnActionCanceled;
        ReloadActionReference.action.started += ReloadActionReference_OnActionStarted;
    }

    private void JumpActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleStartJumpInput();
    }
    private void JumpActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleStopJumpInput();
    }
    private void InteractActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleInteract();
    }
    private void GrabActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleGrabThrow();
    }
    private void AttackActionRereference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleStartAttacking();
    }
    private void AttackActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleStopAttacking();
    }
    private void ReloadActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleReload();
    }

    //JUMP INPUT
    public void HandleStartJumpInput()
    {
        if (CharComponents.CharacterJumping == null) return;
        
        if (CharComponents.CharacterJumping.GetIsAbleToJumpFromFloorOrWall())
        {
            CharComponents.CharacterJumping.TryStartJump();
        }
        else if (CharComponents.CharacterCollision.TimeInAir <= CoyoteLateTimer)
        {
            CharComponents.CharacterJumping.TryStartCoyoteJump();
        }
        else
        {
            _coyoteJumpTooEarlyTimeLeft = CoyoteEarlyTimer;
            _coyoteJumpTooEarlyHandler = StartCoroutine(HandleCoyoteJumpTooEarly());
        }
    }

    private void HandleStopJumpInput()
    {
        if (CharComponents.CharacterJumping == null) return;

        CharComponents.CharacterJumping.StopJump();

        if (_coyoteJumpTooEarlyHandler != null)
        {
            StopCoroutine(_coyoteJumpTooEarlyHandler);
        }
    }

    private IEnumerator HandleCoyoteJumpTooEarly()
    {
        while (_coyoteJumpTooEarlyTimeLeft > 0f)
        {
            _coyoteJumpTooEarlyTimeLeft -= Time.deltaTime;

            if (CharComponents.CharacterJumping.GetIsAbleToJumpFromFloorOrWall())
            {
                CharComponents.CharacterJumping.TryStartJump();
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _coyoteJumpTooEarlyTimeLeft = 0f;
    }

    //INTERACT
    private void HandleInteract()
    {
        if (CurrentSelectedInteractObject != null)
        {
            CurrentSelectedInteractObject.TryInteract(gameObject);
        }
    }

    //GRAB
    private void HandleGrabThrow()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null)
        {
            if (CurrentSelectedGrabObject != null)
            {
                CharComponents.CharacterHolding.TryGrab(CurrentSelectedGrabObject);
            }
        }
        else
        {
            CharComponents.CharacterHolding.TryThrow(CharComponents.CharacterAiming.GetTargetAimNormalized());
        }
    }

    //ATTACK
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

    //RELOAD
    private void HandleReload()
    {
        if (TryGetComponent(out CharacterReloading charReloading))
        {
            charReloading.TryReload();
        }
    }

    //AIM
    private void Update()
    {
        UpdateMoveInput();
        UpdateRollInput();
        UpdateAimInput();
        UpdateAutoAttack();
        UpdateSelectedGrabObject();
        UpdateSelectedInteractObject();
        UpdateAutoReload();
    }

    //MOVE INPUT
    public void UpdateMoveInput()
    {
        if (CharComponents.CharacterMoving == null) return;

        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            float currentInputAxix = MoveActionReference.action.ReadValue<Vector2>().x;
            float roundedInputAxis;
            if (
                (currentInputAxix > 0 && currentInputAxix < MinMoveSpeed) ||
                (currentInputAxix < 0 && currentInputAxix > -MinMoveSpeed)
                )
            {
                roundedInputAxis = 0f;
            }
            else
            {
                roundedInputAxis = currentInputAxix;
            }
            CharComponents.CharacterMoving.TryMove(roundedInputAxis);
        }
        else
        {
            CharComponents.CharacterMoving.TryMove(math.round(MoveActionReference.action.ReadValue<Vector2>().x));
        }
    }

    //ROLL
    private void UpdateRollInput()
    {
        if (
            MoveActionReference.action.ReadValue<Vector2>().y <= -0.5f &&
            math.abs(MoveActionReference.action.ReadValue<Vector2>().x) > 0.05f
            )
        {
            _currentGamepadRollInputDelay += Time.deltaTime;

            if (!CurrentDeviceTracker.GetGamepadIsConnected() || _currentGamepadRollInputDelay > GamePadRollInputDelay)
            {
                if (!_awaitingResetInputToReroll)
                {
                    float rollDirection = MoveActionReference.action.ReadValue<Vector2>().x > 0f ? 1f : -1f;
                    if (CharComponents.CharacterRolling.TryRoll(rollDirection))
                    {
                        _awaitingResetInputToReroll = true;
                    }
                }
            }
        }
        else
        {
            _awaitingResetInputToReroll = false;
            _currentGamepadRollInputDelay = 0f;
        }
    }

    private void UpdateAimInput()
    {
        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            CharComponents.CharacterAiming.TargetAimPoint = CharComponents.Center.transform.position + VectorMath.Vec2ToVec3( AimActionReference.action.ReadValue<Vector2>(), CharComponents.Center.transform.position.z );
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

    private void UpdateSelectedGrabObject()
    {
        if (CharComponents.CharacterHolding != null && CharComponents.CharacterHolding.CurrentHoldObject == null)
        {
            CurrentSelectedGrabObject = 
                CharComponents.CharacterInteract.GetInteractableObjectAtEntireDirection<Holdable>(
                    CharComponents.CharacterAiming.GetCurrentAimNormalized(),
                    1 << CharComponents.CharacterCollision.CurrentZLayer.HoldablesLayer
                );
        }
        else
        {
            CurrentSelectedGrabObject = null;
        }
    }

    private void UpdateSelectedInteractObject()
    {
        if (CharComponents.CharacterInteract != null)
        {
            CurrentSelectedInteractObject =
                CharComponents.CharacterInteract.GetInteractableObjectAtEntireDirection<Interactable>(
                    CharComponents.CharacterAiming.GetCurrentAimNormalized(),
                    CharComponents.CharacterCollision.CurrentZLayer.EntireLayerMask - (1 << CharComponents.CharacterCollision.CurrentZLayer.HoldablesLayer)
                );
        }
    }

    private void UpdateAutoReload()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) && 
            !rangedWeapon.IsReloading &&
            rangedWeapon.GetIsNeedReload()
            )
        {
            CharComponents.CharacterReloading.TryReload();
        }
    }

    private void OnDestroy()
    {
        JumpActionReference.action.started -= JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled -= JumpActionReference_OnActionCanceled;
        InteractActionReference.action.started -= InteractActionReference_OnActionStarted;
    }
}
