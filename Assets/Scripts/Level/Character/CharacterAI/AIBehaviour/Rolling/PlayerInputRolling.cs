using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRolling : AbstractAIRolling
{
    public InputActionReference MoveActionReference;
    public float MinMoveSpeed = 0.5f;

    public float GamePadRollInputDelay = 0.075f;

    private bool _awaitingResetInputToReroll = false;
    private float _currentGamepadRollInputDelay = 0f;

    private void Update()
    {
        if (UIManager.GamePaused()) return;
        UpdateRollInput();
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
}
