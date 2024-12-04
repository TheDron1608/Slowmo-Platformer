using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class CharacterPlayerInputHandler : MonoBehaviour
{
    public InputActionReference MoveActionReference;
    public InputActionReference JumpActionReference;
    public float CoyoteJumpTooEarlyTimer = .33f;
    public float CoyoteJumpTooLateTimer = .125f;

    private float _coyoteJumpTooEarlyTimeLeft = 0f;
    private float _coyoteJumpTooLateTimeLeft = 0f;

    private CharacterActions _characterActionsComponent;
    private Rigidbody2D _rigidbodyComponent;

    private void Awake()
    {
        if (!TryGetComponent<CharacterActions>(out _characterActionsComponent)) throw new UnityException("ChracterActions component not found");
        if (!TryGetComponent<Rigidbody2D>(out _rigidbodyComponent)) throw new UnityException("RigidBody2D component not found");
    }

    private void Update()
    {
        UpdateCoyoteTimers();
        HandleMoveInput();
        HandleJumpInput();
    }

    private void UpdateCoyoteTimers()
    {
        _coyoteJumpTooLateTimeLeft -= Time.deltaTime;
        if (_coyoteJumpTooLateTimeLeft < 0f)
        {
            _coyoteJumpTooLateTimeLeft = 0f;
        }
        _coyoteJumpTooEarlyTimeLeft -= Time.deltaTime;
        if (_coyoteJumpTooEarlyTimeLeft < 0f)
        {
            _coyoteJumpTooEarlyTimeLeft = 0f;
        }


        if (_characterActionsComponent.CharacterJumpingAction.GetIsAbleToJumpFromFloor())
        {
            _coyoteJumpTooLateTimeLeft = CoyoteJumpTooLateTimer;
        }
        else if (JumpActionReference.action.IsInProgress())
        {
            _coyoteJumpTooEarlyTimeLeft = CoyoteJumpTooEarlyTimer;
        }
        else if (!JumpActionReference.action.IsPressed())
        {
            _coyoteJumpTooEarlyTimeLeft = 0f;
        }
    }

    private void HandleMoveInput()
    {
        if (_characterActionsComponent.CharacterMovingAction == null) return;

        _characterActionsComponent.CharacterMovingAction.Move(MoveActionReference.action.ReadValue<Vector2>().x);
    }

    private void HandleJumpInput()
    {
        if (_characterActionsComponent.CharacterJumpingAction == null) return;

        if (JumpActionReference.action.IsInProgress())
        {
            if (_characterActionsComponent.CharacterJumpingAction.GetIsAbleToJumpFromFloor())
            {
                _characterActionsComponent.CharacterJumpingAction.StartJump();
            }
            else if (_coyoteJumpTooLateTimeLeft > 0f)
            {
                _characterActionsComponent.CharacterJumpingAction.ForceStartJump();
                _coyoteJumpTooLateTimeLeft = 0f;
            }
        }
        else if (!JumpActionReference.action.IsPressed())
        {
            _characterActionsComponent.CharacterJumpingAction.StopJump();
        }
    }
}
