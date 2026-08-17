using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRolling : AbstractAIRolling
{
    public InputActionReference RollActionReference;
    public InputActionReference MoveActionReference;
    public float MinMoveSpeed = 0.5f;
    public float CoyoteEarlyTimer = .33f;

    public float GamePadRollInputDelay = 0.075f;

    private bool _awaitingResetInputToReroll = false;
    private float _currentGamepadRollInputDelay = 0f;
    private float? _awaitTooEarlyRollInput = null;

    private void OnEnable()
    {
        RollActionReference.action.started += RollActionReference_OnActionStarted;
    }

    private void RollActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleRollInput();
    }

    private void Update()
    {
        if (UIManager.GamePaused()) return;
        UpdateRollInput();
    }

    //ROLL
    public void HandleRollInput()
    {
        if (!CharComponents.CharacterRolling.TryRoll(CharComponents.CharacterVisual.FlippedH ? -1f : 1f))
        {
            _awaitTooEarlyRollInput = 0f;
        }
    }

    private void UpdateRollInput()
    {
        if (_awaitTooEarlyRollInput.HasValue)
        {
            _awaitTooEarlyRollInput += Time.unscaledDeltaTime;
            if (
                CharComponents.CharacterRolling.TryRoll(CharComponents.CharacterVisual.FlippedH ? -1f : 1f) ||
                _awaitTooEarlyRollInput.Value > CoyoteEarlyTimer
                )
            {
                _awaitTooEarlyRollInput = null;
            }
        }
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
                    if (Camera.main.GetComponent<CameraTrack>().GetCameraFlipped()) rollDirection *= -1;

                    if (CharComponents.CharacterRolling.TryRoll(rollDirection))
                    {
                        _awaitingResetInputToReroll = true;
                    }
                }
            }

            _awaitTooEarlyRollInput = null;
        }
        else
        {
            _awaitingResetInputToReroll = false;
            _currentGamepadRollInputDelay = 0f;
        }
    }

    private void OnDisable()
    {
        RollActionReference.action.started -= RollActionReference_OnActionStarted;
    }
}
