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
        if (UIManager.GamePaused()) return;
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
        else if (CharComponents.CharacterJumping.GetIsAbleToJumpFromAir())
        {
            CharComponents.CharacterJumping.TryStartJump();
        }
        else
        {
            _coyoteJumpTooEarlyTimeLeft = CoyoteEarlyTimer;
            if (CharComponents.gameObject.activeSelf) _coyoteJumpTooEarlyHandler = StartCoroutine(HandleCoyoteJumpTooEarly());
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
        if (UIManager.GamePaused()) return;
        UpdateMoveInput();
    }

    //MOVE INPUT
    public void UpdateMoveInput()
    {
        if (CharComponents.CharacterMoving == null) return;

        float currentInputAxis = MoveActionReference.action.ReadValue<Vector2>().x;

        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            if (math.abs(currentInputAxis) < MinMoveSpeed)
            {
                currentInputAxis = 0f;
            }
        }
        else
        {
            currentInputAxis = math.round(currentInputAxis);
        }

        if (Camera.main.GetComponent<CameraTrack>().GetCameraFlipped()) currentInputAxis *= -1;

        BreakAimIfMoving(currentInputAxis);
        CharComponents.CharacterMoving.TryMove(currentInputAxis);
    }

    private void BreakAimIfMoving(float moveDirection)
    {
        if (
            moveDirection != 0 &&
            CharComponents.CharacterClumsyness.ClumsyRangedAttack &&
            CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.AIM
            )
        {
            CharComponents.CharacterAttacking.BreakClumsyRangedAttack();
        }
    }

    private void OnDestroy()
    {
        JumpActionReference.action.started -= JumpActionReference_OnActionStarted;
        JumpActionReference.action.canceled -= JumpActionReference_OnActionCanceled;
    }
}