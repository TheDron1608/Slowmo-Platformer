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
    private SelectableObject _currentSelectedObject = null;
    private SelectableObject _lastSelectedObject = null;
    private bool _autoAttack = false;
    private bool _awaitingResetInputToReroll = false;
    private float _currentGamepadRollInputDelay = 0f;

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
            CharComponents.CharacterJumping.StartJump();
        }
        else if (CharComponents.CharacterCollisionInfo.TimeInAir <= CoyoteLateTimer)
        {
            CharComponents.CharacterJumping.StartCoyoteJump();
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
                CharComponents.CharacterJumping.StartJump();
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        _coyoteJumpTooEarlyTimeLeft = 0f;
    }

    //INTERACT
    private void InteractWithObjects(InputActionReference playerInputType)
    {
        if (
            _currentSelectedObject != null &&
            _currentSelectedObject.gameObject.TryGetComponent(out Interactable interactComponent) &&
            interactComponent.PlayerInputToInteract == playerInputType
            )
        {
            interactComponent.Interact(gameObject);
        }
        else
        {
            List<SelectableObject> avaibleObjects = CharComponents.CharacterInteract.GetAvaibleInteractableObjects();
            if (avaibleObjects.Count == 0) return;

            var sortedEvaibleObject =
                    from selectableObj in avaibleObjects
                    where selectableObj.PlayerInputToInteract == playerInputType
                    orderby Vector3.Distance(transform.position, selectableObj.transform.position) ascending
                    select selectableObj;

            if (sortedEvaibleObject.Count() != 0)
            {
                if (sortedEvaibleObject.First().TryGetComponent(out Interactable interactableObject))
                {
                    interactableObject.Interact(gameObject);
                }
            }
        }
    }

    private void HandleInteract()
    {
        InteractWithObjects(InteractActionReference);
    }

    //GRAB
    private void HandleGrabThrow()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null)
        {
            InteractWithObjects(GrabActionReference);
        }
        else
        {
            CharComponents.CharacterHolding.TryThrow(CharComponents.CharacterAiming.GetCurrentAimNormalized());
        }
    }

    //ATTACK
    private void HandleStartAttacking()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon))
        {
            CharComponents.CharacterAttacking.TryLoadElseAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());

            if (weapon.PlayerInputAutoAttackOnPress)
            {
                AutoAttack = true;
            }
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
        UpdateSelectedObject();
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
            CharComponents.CharacterMoving.Move(roundedInputAxis);
        }
        else
        {
            CharComponents.CharacterMoving.Move(math.round(MoveActionReference.action.ReadValue<Vector2>().x));
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

    private void UpdateSelectedObject()
    {
        if (CharComponents.CharacterInteract != null)
        {
            _currentSelectedObject = CharComponents.CharacterInteract.GetInteractableObjectAtDirection(CharComponents.CharacterAiming.GetCurrentAimNormalized());

            if (_lastSelectedObject != null && _lastSelectedObject != _currentSelectedObject)
            {
                _lastSelectedObject.Selected = false;
            }

            if (_currentSelectedObject != null)
            {
                _currentSelectedObject.Selected = true;

                _lastSelectedObject = _currentSelectedObject;
            }
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
