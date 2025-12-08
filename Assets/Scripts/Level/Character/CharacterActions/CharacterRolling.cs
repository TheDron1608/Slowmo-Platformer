using System;
using System.Collections.Generic;
using Unity.Mathematics;
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
    public AbstractSoundPlayer SoundOnRoll;
    public AbstractSoundPlayer SoundOnRollHit;

    private bool _isRolling = false;
    private float _currentRollDirection = 0f;
    private float _currentExtraSpeed = 0f;
    private List<AbstractCharacterComponent> _currentRollHitCharacters = new();

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
    public List<AbstractCharacterComponent> CurrentRollHitCharacters
    {
        get => _currentRollHitCharacters;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    private void OnEnable()
    {
        IsRolling = false;
    }

    public bool TryRoll(float direction)
    {

        if (!IsAbleToRoll || IsRolling || !RollCondition(direction))
        {
            return false;
        }

        _currentRollDirection = direction;

        IsRolling = true;

        CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.ROLL;
        CharComponents.CharacterVisual.FlippedH = _currentRollDirection < 0f;

        CharComponents.CharacterAiming.AimWeaponDown = true;

        CharComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier *= AccelerationMultiplier;

        _currentExtraSpeed = ExtraSpeedOnStart;

        OnRoll?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public void ForceStopRolling()
    {
        if (!IsRolling) return;

        IsRolling = false;
        CharComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier /= AccelerationMultiplier;

        if (CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.ROLL)
        {
            CharComponents.CharacterVisual.BreakBusyAnimation();
            CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.NONE;
        }

        if (!CharComponents.CharacterClumsyness.ClumsyRangedAttack)
        {
            CharComponents.CharacterAiming.AimWeaponDown = false;
        }

        _currentRollHitCharacters = new();
        OnFinishRoll?.Invoke(this, EventArgs.Empty);
    }

    private bool RollCondition(float direction)
    {
        return
            (
                (direction > 0 && !CharComponents.CharacterCollision.GetTileBehaviourTypeFromRightWall().HasValue) ||
                (direction < 0 && !CharComponents.CharacterCollision.GetTileBehaviourTypeFromLeftWall().HasValue)
            ) &&
            CharComponents.CharacterCollision.IsCollidingFloor();
    }

    private void FixedUpdate()
    {
        if (IsRolling)
        {
            if (RollCondition(_currentRollDirection))
            {
                CharComponents.CharacterRigidBody.linearVelocityX =
                    math.lerp(
                        CharComponents.CharacterRigidBody.linearVelocityX,
                        (RollSpeed + _currentExtraSpeed) * _currentRollDirection,
                        (RollSpeed + _currentExtraSpeed) * Time.fixedDeltaTime
                        );

                _currentExtraSpeed -= Time.fixedDeltaTime * ExtraSpeedOnStart / ExtraSpeedDuration;
                if (_currentExtraSpeed < 0f)
                {
                    _currentExtraSpeed = 0f;
                }
            }
            else
            {
                ForceStopRolling();
            }
        }
    }
}
