using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterRolling : MonoBehaviour
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

    private CharacterVisual _characterVisualComponent;
    private CharacterActions _characterActionsComponent;
    private Rigidbody2D _characterRigidBodyComponent;
    private CharacterCollisionInfo _characterCollisionInfoComponent;

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

    private void Awake()
    {
        if (!TryGetComponent(out _characterVisualComponent)) throw new UnityException("CharacterVisual component not found");
        if (!TryGetComponent(out _characterActionsComponent)) throw new UnityException("CharacterActions component not found");
        if (!TryGetComponent(out _characterRigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _characterCollisionInfoComponent)) throw new UnityException("CharacterCollisionInfo component not found");
        _characterVisualComponent.OnBusyAnimationFinished += CharacterVisual_OnBusyAnimationFinished;
        _characterCollisionInfoComponent.OnCollisionChanged += CharacterCollisionInfo_OnCollisionChanged;
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
            _characterActionsComponent.CharacterMovingAction.SpeedAccelerationOnGroundMultiplier /= AccelerationMultiplier;
            _characterActionsComponent.CharacterMovingAction.IsAbleToMove = true;
        }
    }

    public bool TryRoll(float direction)
    {
        _currentRollDirection = direction;
        IsRolling = true;

        if (!IsAbleToRoll || _characterVisualComponent.IsBusy() || !RollCondition())
        {
            IsRolling = false;
            return false;
        }

        _characterActionsComponent.CharacterMovingAction.IsAbleToMove = false;

        _characterVisualComponent.CurrentBusyAnimation = CharacterPart.CharacterPartBusyStates.ROLL;
        _characterVisualComponent.SpritesFlipped = _currentRollDirection < 0f;

        _characterActionsComponent.CharacterMovingAction.SpeedAccelerationOnGroundMultiplier *= AccelerationMultiplier;

        _currentExtraSpeed = ExtraSpeedOnStart;

        OnRoll?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public void ForceStopRolling()
    {
        if (!IsRolling) return;

        _characterVisualComponent.BreakBusyAnimation();
        OnFinishRoll?.Invoke(this, EventArgs.Empty);
    }

    private bool RollCondition()
    {
        return
            (
                (_currentRollDirection > 0 && !_characterCollisionInfoComponent.IsCollidingRightWall()) ||
                (_currentRollDirection < 0 && !_characterCollisionInfoComponent.IsCollidingLeftWall())
            ) &&
            _characterCollisionInfoComponent.IsCollidingFloor();
    }

    private void Update()
    {
        if (!IsRolling) return;

        _characterRigidBodyComponent.linearVelocityX = 
            math.lerp(
                _characterRigidBodyComponent.linearVelocityX, 
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
