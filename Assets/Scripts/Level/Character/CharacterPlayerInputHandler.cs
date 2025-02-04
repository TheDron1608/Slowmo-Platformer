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
public class CharacterPlayerInputHandler : MonoBehaviour
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

    private CharacterActions _characterActionsComponent;
    private Rigidbody2D _rigidbodyComponent;
    private CharacterCollisionInfo _characterInfoComponent;
    private CharacterChildNodes _characterChildNodesComponent;
    private CharacterHoldingObjects _characterHoldingObjectsComponent;

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

    private void Awake()
    {
        if (!TryGetComponent(out _characterActionsComponent)) throw new UnityException("ChracterActions component not found");
        if (!TryGetComponent(out _rigidbodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _characterInfoComponent)) throw new UnityException("CharacterInfo component not found");
        if (!TryGetComponent(out _characterChildNodesComponent)) throw new UnityException("CharacterChildNodes component not found");
        if (!TryGetComponent(out _characterHoldingObjectsComponent)) throw new UnityException("CharacterHoldingObject component not found");
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
        if (_characterActionsComponent.CharacterJumpingAction == null) return;
        
        if (_characterActionsComponent.CharacterJumpingAction.GetIsAbleToJumpFromFloorOrWall())
        {
            _characterActionsComponent.CharacterJumpingAction.StartJump();
        }
        else if (_characterInfoComponent.TimeInAir <= CoyoteLateTimer)
        {
            _characterActionsComponent.CharacterJumpingAction.ForceStartJump();
        }
        else
        {
            _coyoteJumpTooEarlyTimeLeft = CoyoteEarlyTimer;
            _coyoteJumpTooEarlyHandler = StartCoroutine(HandleCoyoteJumpTooEarly());
        }
    }

    private void HandleStopJumpInput()
    {
        if (_characterActionsComponent.CharacterJumpingAction == null) return;

        _characterActionsComponent.CharacterJumpingAction.StopJump();

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

            if (_characterActionsComponent.CharacterJumpingAction.GetIsAbleToJumpFromFloorOrWall())
            {
                _characterActionsComponent.CharacterJumpingAction.StartJump();
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
            List<SelectableObject> avaibleObjects = _characterActionsComponent.CharacterInteractAction.GetAvaibleInteractableObjects();
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
        if (_characterActionsComponent.CharacterHoldingAction.CurrentHoldObject == null)
        {
            InteractWithObjects(GrabActionReference);
        }
        else
        {
            _characterActionsComponent.CharacterHoldingAction.TryThrow(_characterActionsComponent.CharacterAimingAction.GetCurrentAimNormalized());
        }
    }

    //ATTACK
    private void HandleStartAttacking()
    {
        if (_characterHoldingObjectsComponent.CurrentHoldObject != null && _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out Weapon weapon))
        {
            _characterActionsComponent.CharacterAttackingAction.TryLoadElseAttack(_characterActionsComponent.CharacterAimingAction.GetCurrentAimNormalized());

            if (weapon.PlayerInputAutoAttackOnPress)
            {
                AutoAttack = true;
            }
        }
    }

    private void HandleStopAttacking()
    {
        if (
            _characterHoldingObjectsComponent.CurrentHoldObject != null && 
            _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon)
            )
        {
            _characterActionsComponent.CharacterAttackingAction.TryAttack(_characterActionsComponent.CharacterAimingAction.GetCurrentAimNormalized());
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
        if (_characterActionsComponent.CharacterMovingAction == null) return;

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
            _characterActionsComponent.CharacterMovingAction.Move(roundedInputAxis);
        }
        else
        {
            _characterActionsComponent.CharacterMovingAction.Move(math.round(MoveActionReference.action.ReadValue<Vector2>().x));
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
                    if (_characterActionsComponent.CharacterRollingAction.TryRoll(rollDirection))
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
            _characterActionsComponent.CharacterAimingAction.TargetAimPoint = _characterChildNodesComponent.Center.transform.position + VectorMath.Vec2ToVec3( AimActionReference.action.ReadValue<Vector2>(), _characterChildNodesComponent.Center.transform.position.z );
        }
        else
        {
            Vector3? mousePos = GetMouseWorldPositionOnCharacterLayer();
            if (mousePos.HasValue)
            {
                _characterActionsComponent.CharacterAimingAction.TargetAimPoint = mousePos.Value;
            }
        }
    }

    private void UpdateAutoAttack()
    {
        if (
            AutoAttack &&
            _characterHoldingObjectsComponent.CurrentHoldObject != null &&
            _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out Weapon weapon) &&
            _characterActionsComponent.CharacterAttackingAction != null
            )
        {
            _characterActionsComponent.CharacterAttackingAction.TryLoadElseAttack(_characterActionsComponent.CharacterAimingAction.GetCurrentAimNormalized());
                
            if (weapon.TryGetComponent(out RangedWeapon rangedWeapon) && rangedWeapon.GetIsOutOfAmmo())
            {
                AutoAttack = false;
            }
        }
    }

    private void UpdateSelectedObject()
    {
        if (_characterActionsComponent.CharacterInteractAction != null)
        {
            _currentSelectedObject = _characterActionsComponent.CharacterInteractAction.GetInteractableObjectAtDirection(_characterActionsComponent.CharacterAimingAction.GetCurrentAimNormalized());

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
            _characterHoldingObjectsComponent.CurrentHoldObject != null &&
            _characterHoldingObjectsComponent.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) && 
            !rangedWeapon.IsReloading &&
            rangedWeapon.GetIsNeedReload()
            )
        {
            _characterActionsComponent.CharacterReloadingAction.TryReload();
        }
    }

    private void OnDestroy()
    {
        JumpActionReference.action.started -= JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled -= JumpActionReference_OnActionCanceled;
        InteractActionReference.action.started -= InteractActionReference_OnActionStarted;
    }
}
