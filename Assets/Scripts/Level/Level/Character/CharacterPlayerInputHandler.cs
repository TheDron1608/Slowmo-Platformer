using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[Serializable]
public class CharacterPlayerInputHandler : MonoBehaviour
{
    public InputActionReference MoveActionReference;
    public InputActionReference JumpActionReference;
    public InputActionReference AimActionReference;
    public InputActionReference InteractActionReference;
    public InputActionReference GrabActionReference;
    public float MinMoveSpeed = 0.5f;

    private Coroutine MoveGamepadActionHandler;

    public float CoyoteJumpTooEarlyTimer = .33f;
    public float CoyoteJumpTooLateTimer = .125f;

    private float _coyoteJumpTooEarlyTimeLeft = 0f;
    private Coroutine _coyoteJumpTooEarlyHandler;
    private SelectableObject _currentSelectedObject = null;
    private SelectableObject _lastSelectedObject = null;
    private Vector2 _currentAimDirection = Vector2.zero;

    private CharacterActions _characterActionsComponent;
    private Rigidbody2D _rigidbodyComponent;
    private CharacterCollisionInfo _characterInfoComponent;

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
        if (!TryGetComponent<CharacterActions>(out _characterActionsComponent)) throw new UnityException("ChracterActions component not found");
        if (!TryGetComponent<Rigidbody2D>(out _rigidbodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CharacterCollisionInfo>(out _characterInfoComponent)) throw new UnityException("CharacterInfo component not found");
    }

    private void Start()
    {
        MoveActionReference.action.started += MoveActionReference_OnActionStarted;
        MoveActionReference.action.canceled += MoveActionReference_OnActionCanceled;
        JumpActionReference.action.started += JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled += JumpActionReference_OnActionCanceled;
        InteractActionReference.action.started += InteractActionReference_OnActionStarted;
        GrabActionReference.action.started += GrabActionReference_OnActionStarted;
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
    private void InteractActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleInteract();
    }
    private void GrabActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleGrabThrow();
    }

    //MOVE INPUT
    public void HandleMoveInput()
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
    public void HandleStartJumpInput()
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

    //INTERACT
    public void HandleInteract()
    {
        if (_currentSelectedObject != null && _currentSelectedObject.gameObject.TryGetComponent(out Interactable interactComponent))
        {
            interactComponent.Interact(gameObject);
        }
    }

    //GRAB
    public void HandleGrabThrow()
    {
        if (_characterActionsComponent.CharacterHoldingAction.CurrentHoldObject == null)
        {
            if (_currentSelectedObject != null && _currentSelectedObject.TryGetComponent(out Holdable holdableObj))
            {
                _characterActionsComponent.CharacterHoldingAction.TryGrab(holdableObj);
            }
        }
        else
        {
            _characterActionsComponent.CharacterHoldingAction.TryThrow(_currentAimDirection);
        }
    }

    //AIM
    private void Update()
    {
        UpdateAimInput();
        UpdateSelectedObject();
    }

    public void UpdateAimInput()
    {
        Vector2 aimDirection;

        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            aimDirection = AimActionReference.action.ReadValue<Vector2>();
        }
        else
        {
            Vector3? mousePos = GetMouseWorldPositionOnCharacterLayer();

            if (!mousePos.HasValue) return;


            aimDirection = new Vector2(
                mousePos.Value.x - transform.position.x,
                mousePos.Value.y - transform.position.y
            ).normalized;
        }

        _currentAimDirection = aimDirection;
    }

    private void UpdateSelectedObject()
    {
        if (_characterActionsComponent.CharacterInteractAction != null)
        {
            _currentSelectedObject = _characterActionsComponent.CharacterInteractAction.GetInteractableObjectAtDirection(_currentAimDirection);

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

    private void OnDestroy()
    {
        MoveActionReference.action.started -= MoveActionReference_OnActionStarted;
        MoveActionReference.action.canceled -= MoveActionReference_OnActionCanceled;
        JumpActionReference.action.started -= JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled -= JumpActionReference_OnActionCanceled;
        InteractActionReference.action.started -= InteractActionReference_OnActionStarted;
    }
}
