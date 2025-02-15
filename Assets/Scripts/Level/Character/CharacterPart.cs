using System;
using UnityEngine;

public class CharacterPart : MonoBehaviour
{
    public const string ANIMATOR_MAIN_STATE_PARAM_NAME = "MainState";
    public const string ANIMATOR_MOVE_SPEED_PARAM_NAME = "MoveSpeed";
    public const string ANIMATOR_JUMP_STATE_PARAM_NAME = "JumpState";
    public const string ANIMATOR_BUSY_STATE_PARAM_NAME = "BusyState";
    public const string ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME = "BreakBusyAnimation";

    public enum CharacterPartMainStates
    {
        IDLE = 0,
        MOVE = 1,
        JUMP = 2,
        SLIDE_ON_WALL = 3,
    }
    public enum CharacterPartBusyStates
    {
        NONE = 0,
        LOOK_FORWARD = 1,
        LOOK_BACKWARD = 2,
        LOOK_FORWARD_REVERSED = 3,
        LOOK_BACKWARD_REVERSED = 4,
        ROLL = 5,
        FALLING_IN_AIR = 6,
        FALLEN_ON_FLOOR = 7
    }


    private Animator _animatorComponent;

    private void Awake()
    {
        if (!TryGetComponent(out _animatorComponent)) throw new UnityException("Animator component not found");
    }

    public void SetMainState(CharacterPartMainStates newState)
    {
        _animatorComponent.SetInteger(ANIMATOR_MAIN_STATE_PARAM_NAME, (int)newState);
    }

    public void SetJumpState(float value)
    {
        float normalizedTime = value;

        //converts range [-inf; +inf] into (-1; 1)
        if (normalizedTime < -0.95f) normalizedTime = -0.95f;
        else if (normalizedTime > 0.95f) normalizedTime = 0.95f;

        normalizedTime = 1f - (normalizedTime + 1f) / 2f; //converts range (-1; 1) into (1; 0)

        _animatorComponent.SetFloat(ANIMATOR_JUMP_STATE_PARAM_NAME, normalizedTime);
    }

    public void SetMoveSpeed(float value)
    {
        _animatorComponent.SetFloat(ANIMATOR_MOVE_SPEED_PARAM_NAME, value);
    }

    public void SetBusyState(CharacterPartBusyStates value)
    {
        _animatorComponent.SetInteger(ANIMATOR_BUSY_STATE_PARAM_NAME, (int)value);
    }

    public void SetBreakBusyAnimationTrigger()
    {
        _animatorComponent.SetTrigger(ANIMATOR_BREAK_BUSY_ANIMATION_TRIGGER_NAME);
    }
}
