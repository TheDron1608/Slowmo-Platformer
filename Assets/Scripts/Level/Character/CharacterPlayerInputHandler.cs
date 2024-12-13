using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class CharacterPlayerInputHandler : MonoBehaviour
{
    public InputActionReference MoveActionReference;
    public InputActionReference JumpActionReference;
    public InputActionReference AimActionReference;
    public float MinMoveSpeed = 0.5f;

    private Coroutine MoveGamepadActionHandler;

    public float CoyoteJumpTooEarlyTimer = .33f;
    public float CoyoteJumpTooLateTimer = .125f;

    private float _coyoteJumpTooEarlyTimeLeft = 0f;
    private Coroutine _coyoteJumpTooEarlyHandler;
    private InteractableObject _lastSelectedObject = null;

    private CharacterActions _characterActionsComponent;
    private Rigidbody2D _rigidbodyComponent;
    private CollisionCharacterInfo _characterInfoComponent;

    private void Awake()
    {
        if (!TryGetComponent<CharacterActions>(out _characterActionsComponent)) throw new UnityException("ChracterActions component not found");
        if (!TryGetComponent<Rigidbody2D>(out _rigidbodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CollisionCharacterInfo>(out _characterInfoComponent)) throw new UnityException("CharacterInfo component not found");
    }

    private void Start()
    {
        MoveActionReference.action.started += MoveActionReference_OnActionStarted;
        MoveActionReference.action.canceled += MoveActionReference_OnActionCanceled;
        JumpActionReference.action.started += JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled += JumpActionReference_OnActionCanceled;
    }

    private void MoveActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleMoveInput();
    }

    private void MoveActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleMoveInput();
    }

    private void JumpActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleStartJumpInput();
    }

    private void JumpActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleStopJumpInput();
    }

    //MOVE INPUT
    private void HandleMoveInput()
    {
        if (_characterActionsComponent.CharacterMovingAction == null) return;

        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            MoveGamepadActionHandler = StartCoroutine(MoveGamepadAction());
        }
        else
        {
            _characterActionsComponent.CharacterMovingAction.Move(MoveActionReference.action.ReadValue<Vector2>().x);
        }
    }
    private IEnumerator MoveGamepadAction()
    {
        float currentInputAxix;
        float roundedInputAxis;
        do
        {
            currentInputAxix = MoveActionReference.action.ReadValue<Vector2>().x;
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
            yield return new WaitForEndOfFrame();
        }
        while (currentInputAxix != 0f);
    }
    
    //JUMP INPUT
    private void HandleStartJumpInput()
    {
        if (_characterActionsComponent.CharacterJumpingAction == null) return;
        
        if (_characterActionsComponent.CharacterJumpingAction.GetIsAbleToJumpFromFloorOrWall())
        {
            _characterActionsComponent.CharacterJumpingAction.StartJump();
        }
        else if (_characterInfoComponent.TimeInAir <= CoyoteJumpTooLateTimer)
        {
            _characterActionsComponent.CharacterJumpingAction.ForceStartJump();
        }
        else
        {
            _coyoteJumpTooEarlyTimeLeft = CoyoteJumpTooEarlyTimer;
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

    private void Update()
    {
        HandleAimInput();
    }

    private void HandleAimInput()
    {
        Vector2 aimDirection;

        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            aimDirection = AimActionReference.action.ReadValue<Vector2>();
        }
        else
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            aimDirection = new Vector2(
                mousePos.x - transform.position.x,
                mousePos.y - transform.position.y
            ).normalized;
        }

        if (_characterActionsComponent.CharacterInteractAction != null)
        {
            var currentSelectedObj = _characterActionsComponent.CharacterInteractAction.GetInteractableObjectAtDirection(aimDirection);

            if (_lastSelectedObject != null && _lastSelectedObject != currentSelectedObj)
            {
                _lastSelectedObject.Selected = false;
            }

            if (currentSelectedObj != null)
            {
                currentSelectedObj.Selected = true;

                _lastSelectedObject = currentSelectedObj;
            }
        }
    }

    private void OnDestroy()
    {
        MoveActionReference.action.started -= MoveActionReference_OnActionStarted;
        MoveActionReference.action.canceled -= MoveActionReference_OnActionCanceled;
        JumpActionReference.action.started -= JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled -= JumpActionReference_OnActionCanceled;
    }
}
