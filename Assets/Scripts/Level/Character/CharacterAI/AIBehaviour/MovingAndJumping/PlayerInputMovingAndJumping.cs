using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMovingAndJumping : AbstractAIMovingAndJumping
{
    public InputActionReference MoveActionReference;
    public InputActionReference JumpActionReference;
    public float MinMoveSpeed = 0.5f;

    public float CoyoteEarlyTimer = .33f;
    public float CoyoteLateTimer = .125f;

    private float _coyoteJumpTooEarlyTimeLeft = 0f;
    private Coroutine _coyoteJumpTooEarlyHandler;

    private void Start()
    {
        JumpActionReference.action.started += JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled += JumpActionReference_OnActionCanceled;
    }

    private void JumpActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleStartJumpInput();
    }
    private void JumpActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        HandleStopJumpInput();
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

    private void Update()
    {
        UpdateMoveInput();
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

    private void OnDestroy()
    {
        JumpActionReference.action.started -= JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled -= JumpActionReference_OnActionCanceled;
    }
}