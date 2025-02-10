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
        _charComponents.CharacterVisual.OnBusyAnimationFinished += CharacterVisual_OnBusyAnimationFinished;
        _charComponents.CharacterCollisionInfo.OnCollisionChanged += CharacterCollisionInfo_OnCollisionChanged;
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
            _charComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier /= AccelerationMultiplier;
            _charComponents.CharacterMoving.IsAbleToMove = true;
        }
    }

    public bool TryRoll(float direction)
    {
        _currentRollDirection = direction;
        IsRolling = true;

        if (!IsAbleToRoll || _charComponents.CharacterVisual.IsBusy() || !RollCondition())
        {
            IsRolling = false;
            return false;
        }

        _charComponents.CharacterMoving.IsAbleToMove = false;

        _charComponents.CharacterVisual.CurrentBusyAnimation = CharacterPart.CharacterPartBusyStates.ROLL;
        _charComponents.CharacterVisual.SpritesFlipped = _currentRollDirection < 0f;

        _charComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier *= AccelerationMultiplier;

        _currentExtraSpeed = ExtraSpeedOnStart;

        OnRoll?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public void ForceStopRolling()
    {
        if (!IsRolling) return;

        _charComponents.CharacterVisual.BreakBusyAnimation();
        OnFinishRoll?.Invoke(this, EventArgs.Empty);
    }

    private bool RollCondition()
    {
        return
            (
                (_currentRollDirection > 0 && !_charComponents.CharacterCollisionInfo.IsCollidingRightWall()) ||
                (_currentRollDirection < 0 && !_charComponents.CharacterCollisionInfo.IsCollidingLeftWall())
            ) &&
            _charComponents.CharacterCollisionInfo.IsCollidingFloor();
    }

    private void Update()
    {
        if (!IsRolling) return;

        _charComponents.CharacterRigidBody.linearVelocityX = 
            math.lerp(
                _charComponents.CharacterRigidBody.linearVelocityX, 
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
