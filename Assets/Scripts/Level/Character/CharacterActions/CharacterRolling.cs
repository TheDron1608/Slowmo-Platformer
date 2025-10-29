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
        CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
        CharComponents.CharacterCollision.OnCollisionChanged += CharacterCollisionInfo_OnCollisionChanged;
    }

    private void CharacterCollisionInfo_OnCollisionChanged(object sender, CharacterCollision.OnCollisionChangedEventArgs e)
    {
        if (!RollCondition())
        {
            ForceStopRolling();
        }
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.OldState == CharacterVisual.CharacterPartBusyStates.ROLL)
        {
            IsRolling = false;
            CharComponents.CharacterMoving.SpeedAccelerationOnGroundMultiplier /= AccelerationMultiplier;

            if (!CharComponents.CharacterClumsyness.ClumsyRangedAttack)
            {
                CharComponents.CharacterAiming.AimWeaponDown = false;
            }

            _currentRollHitCharacters = new();
            OnFinishRoll?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool TryRoll(float direction)
    {
        _currentRollDirection = direction;

        if (!IsAbleToRoll || IsRolling || !RollCondition())
        {
            return false;
        }

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

        CharComponents.CharacterVisual.BreakBusyAnimation();

        if (!CharComponents.CharacterClumsyness.ClumsyRangedAttack)
        {
            CharComponents.CharacterAiming.AimWeaponDown = false;
        }
        _currentRollHitCharacters = new();
        OnFinishRoll?.Invoke(this, EventArgs.Empty);
    }

    private bool RollCondition()
    {
        return
            (
                (_currentRollDirection > 0 && !CharComponents.CharacterCollision.IsCollidingRightWall()) ||
                (_currentRollDirection < 0 && !CharComponents.CharacterCollision.IsCollidingLeftWall())
            ) &&
            CharComponents.CharacterCollision.IsCollidingFloor();
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
