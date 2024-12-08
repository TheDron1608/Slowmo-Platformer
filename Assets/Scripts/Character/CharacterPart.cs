using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterPart : MonoBehaviour
{
    const string ANIMATOR_MAIN_STATE_PARAM_NAME = "MainState";
    const string ANIMATOR_IS_GROUNDE_PARAM_NAME = "IsGrounded";
    const string ANIMATOR_MOVE_SPEED_PARAM_NAME = "MoveSpeed";
    const string ANIMATOR_JUMP_STATE_PARAM_NAME = "JumpState";

    public enum CharacterPartMainStates
    {
        IDLE = 0,
        MOVE = 1,
    }


    private Animator _animatorComponent;

    private void Awake()
    {
        if (!TryGetComponent<Animator>(out _animatorComponent)) throw new UnityException("Animator component not found");
    }

    public void SetMainState(CharacterPartMainStates newState)
    {
        _animatorComponent.SetInteger(ANIMATOR_MAIN_STATE_PARAM_NAME, (int)newState);
    }

    public void SetMainState(int newState)
    {
        _animatorComponent.SetInteger(ANIMATOR_MAIN_STATE_PARAM_NAME, newState);
    }

    public void SetIsGrounded(bool value)
    {
        _animatorComponent.SetBool(ANIMATOR_IS_GROUNDE_PARAM_NAME, value);
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
}
