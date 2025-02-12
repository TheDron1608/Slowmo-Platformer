using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterRolling : AbstractCharacterComponent
{
    public enum RollDirection
    {
        None = 0,
        Left = -1,
        Right = 1,
    }

    public bool IsAbleToRoll = true;
    public float RollSpeed = 0f;
    public float ExtraSpeedOnStart = 5f;
    public float ExtraSpeedDuration = 0.35f; //in seconds
    public float AccelerationMultiplier = 0.2f;

    private bool _isRolling = false;
    private float _currentRollDirection = 0f;
    private float _currentExtraSpeed = 0f;

    public event EventHandler OnRoll;
    public event EventHandler OnFinishRoll;

    public bool IsRolling
    {
        get => _isRolling;
        private set
        {
            _isRolling = value;
            if (!value)
            {
                _currentRollDirection = 0f;
            }
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterVisual.OnBusyAnimationFinished += CharacterVisual_OnBusyAnimationFinished;
        CharComponents.CharacterCollisionInfo.OnCollisionChanged += CharacterCollisionInfo_OnCollisionChanged;
    }

    private void CharacterCollisionInfo_OnCollisionChanged(object sender, CharacterCollisionInfo.OnCollisionChangedEventArgs e)
    {
        if (!RollCondition())
        {
            ForceStopRolling();
        }
    }

    private void CharacterVisual_OnBusyAnimationFinished(object sender, CharacterPart.CharacterPartBusyStates e)
    {
        if (e == CharacterPart.CharacterPartBusyStates.ROLL)
        {
            IsRolling = false;
            CharComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier /= AccelerationMultiplier;
            CharComponents.CharacterMoving.IsAbleToMove = true;
        }
    }

    public bool TryRoll(float direction)
    {
        _currentRollDirection = direction;
        IsRolling = true;

        if (!IsAbleToRoll || CharComponents.CharacterVisual.IsBusy() || !RollCondition())
        {
            IsRolling = false;
            return false;
        }

        CharComponents.CharacterMoving.IsAbleToMove = false;

        CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterPart.CharacterPartBusyStates.ROLL;
        CharComponents.CharacterVisual.SpritesFlipped = _currentRollDirection < 0f;

        CharComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier *= AccelerationMultiplier;

        _currentExtraSpeed = ExtraSpeedOnStart;

        OnRoll?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public void ForceStopRolling()
    {
        if (!IsRolling) return;

        CharComponents.CharacterVisual.BreakBusyAnimation();
        OnFinishRoll?.Invoke(this, EventArgs.Empty);
    }

    private bool RollCondition()
    {
        return
            (
                (_currentRollDirection > 0 && !CharComponents.CharacterCollisionInfo.IsCollidingRightWall()) ||
                (_currentRollDirection < 0 && !CharComponents.CharacterCollisionInfo.IsCollidingLeftWall())
            ) &&
            CharComponents.CharacterCollisionInfo.IsCollidingFloor();
    }

    private void Update()
    {
        if (!IsRolling) return;

        CharComponents.CharacterRigidBody.linearVelocityX = 
            math.lerp(
                CharComponents.CharacterRigidBody.linearVelocityX, 
                (RollSpeed + _currentExtraSpeed) * _currentRollDirection, 
                (RollSpeed + _currentExtraSpeed) * Time.fixedDeltaTime
                );

        _currentExtraSpeed -= Time.deltaTime * ExtraSpeedOnStart / ExtraSpeedDuration;
        if (_currentExtraSpeed < 0f)
        {
            _currentExtraSpeed = 0f;
        }
    }
}
