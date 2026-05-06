using System;
using Unity.Mathematics;
using UnityEngine;

public class CharacterMoving : AbstractCharacterComponent
{
    public enum MoveDirection
    {
        None = 0,
        Left = -1,
        Right = 1
    }

    private float _currentMoveDirection;
    [SerializeField] private bool _isAbleToMove = true;
    private float _lastMoveDirection = 0f;
    private float _lastActiveMoveDirection = 0f;
    private float? _awaitingMoveDirection = null;

    [Header("Sound")]
    public AbstractSoundPlayer StepSound;
    public AbstractSoundPlayer MoveAlignChangeSound;

    public bool IsAbleToMove
    {
        get => _isAbleToMove;
        set
        {
            _isAbleToMove = value;

            if (value)
            {
                TryMove(_lastMoveDirection);
            }
            else
            {
                _currentMoveDirection = 0f;
                OnMoveAlignChanged?.Invoke(this, _currentMoveDirection);
            }
        }
    }
    public float LastMoveDirection
    {
        get => _lastMoveDirection;
    }

    public float LastActiveMoveDirection
    {
        get => _lastActiveMoveDirection;
    }

    public float Speed = 5f;
    public float SpeedAccelerationOnGroundMultiplier = 5f;
    public float SpeedAccelerationOnAirMulitplier = 1f;
    public float SpeedAccelerationOnUnableToMoveMultiplier = 0.33f;

    public event EventHandler<float> OnMoveAlignChanged;
    public event EventHandler<float> OnReachedMaxSpeed;

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
        _lastActiveMoveDirection = NumberMath.RandomCoinflip() ? -1f : 1f;
    }

    private void OnEnable()
    {
        _currentMoveDirection = 0f;
    }

    public bool IsMoving()
    {
        return Math.Abs(CharComponents.CharacterMoving.GetCurrentMoveDirection()) > 0.05f;
    }

    private void FixedUpdate()
    {
        UpdateMoving();
    }

    private void UpdateMoving()
    {
        bool isAlreadyReachedMaxSpeed = GetIsMaxSpeed();

        if (_currentMoveDirection > 0f && CharComponents.CharacterCollision.IsCollidingRightWall())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityX > 0) CharComponents.CharacterRigidBody.linearVelocityX = 0f;
            OnMoveAlignChanged?.Invoke(this, 0f);
        }
        else if (_currentMoveDirection < 0f && CharComponents.CharacterCollision.IsCollidingLeftWall())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityX < 0) CharComponents.CharacterRigidBody.linearVelocityX = 0f;
            OnMoveAlignChanged?.Invoke(this, 0f);
        }
        else
        {
            if (!IsAbleToMove)
            {
                CharComponents.CharacterRigidBody.linearVelocityX = math.lerp(CharComponents.CharacterRigidBody.linearVelocityX, _currentMoveDirection * Speed, NumberMath.LimitFloatBetweenZeroAndOne(Speed * SpeedAccelerationOnUnableToMoveMultiplier * Time.fixedDeltaTime));
            }
            else if (CharComponents.CharacterCollision.IsCollidingFloor() && !CharComponents.CharacterJumping.GetIsJumping())
            {
                CharComponents.CharacterRigidBody.linearVelocityX = math.lerp(CharComponents.CharacterRigidBody.linearVelocityX, _currentMoveDirection * Speed, NumberMath.LimitFloatBetweenZeroAndOne(Speed * SpeedAccelerationOnGroundMultiplier * Time.fixedDeltaTime));
            }
            else if (GetCurrentSpeedIsOverMaxMoveSpeed())
            {
                CharComponents.CharacterRigidBody.linearVelocityX = math.lerp(CharComponents.CharacterRigidBody.linearVelocityX, _currentMoveDirection * Speed, NumberMath.LimitFloatBetweenZeroAndOne(Speed * SpeedAccelerationOnAirMulitplier * Time.fixedDeltaTime));
            }
        }

        if (!isAlreadyReachedMaxSpeed && GetIsMaxSpeed())
        {
            OnReachedMaxSpeed?.Invoke(this, _currentMoveDirection);
        }
    }

    private bool GetCurrentSpeedIsOverMaxMoveSpeed()
    {
        return
            (_currentMoveDirection == 0f) ||
            (_currentMoveDirection > 0f && CharComponents.CharacterRigidBody.linearVelocityX < Speed) ||
            (_currentMoveDirection < 0f && CharComponents.CharacterRigidBody.linearVelocityX > -Speed);
    }

    /// <summary>
    /// Character moves horizontally with "align" speed
    /// </summary>
    /// <param name="direction">Value between -1 and 1</param>
    public void TryMove(float direction)
    {
        if (
            IsAbleToMove && 
            (!CharComponents.CharacterVisual.IsBusy() || CharComponents.CharacterVisual.AllowMovementOnBusyAnimation)
            )
        {
            if (GetIsNeedChangeClumsyDirection(direction))
            {
                TrySetClumsyAlign(direction, false);
            }
            else
            {
                ForceMove(direction);
            }
        }
    }

    public void TryMove(MoveDirection direction)
    {
        TryMove((float)direction);
    }

    public void ForceMove(float direction)
    {
        if (GetIsNeedChangeFastDirection(direction))
        {
            CharComponents.CharacterVisual.FastMoveAlignChange();
        }

        _lastMoveDirection = direction;
        if (direction != 0f)
        {
            _lastActiveMoveDirection = direction;
        }

        if (!_isAbleToMove) return;
        if (_currentMoveDirection == direction && _lastMoveDirection == direction) return;

        OnMoveAlignChanged?.Invoke(this, direction);

        _currentMoveDirection = direction;
    }

    public void TrySetClumsyAlign(float direction, bool flipHOnly)
    {
        if (GetIsNeedChangeClumsyDirection(direction))
        {
            CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.CLUMSY_MOVE_ALIGN_CHANGE;
            _awaitingMoveDirection = flipHOnly ? 0f : direction;
        }
    }

    public bool GetIsNeedChangeClumsyDirection(float direction)
    {
        return
            _awaitingMoveDirection != direction &&
            CharComponents.CharacterClumsyness.ClumsyMovement &&
            (CharComponents.CharacterVisual.FlippedH ^ direction < 0f) &&
            direction != 0f &&
            !CharComponents.CharacterVisual.IsBusy() &&
            IsAbleToMove;
    }

    public bool GetIsNeedChangeFastDirection(float direction)
    {
        return
           CharComponents.CharacterCollision.IsCollidingFloor() &&
           !CharComponents.CharacterClumsyness.ClumsyMovement &&
           (CharComponents.CharacterVisual.FlippedH ^ direction < 0f) &&
           direction != 0f &&
           !CharComponents.CharacterVisual.IsBusy() &&
           IsAbleToMove;
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.NewState != CharacterVisual.CharacterPartBusyStates.NONE && !CharComponents.CharacterVisual.AllowMovementOnBusyAnimation)
        {
            ForceMove(0f);
        }
        else if (e.OldState == CharacterVisual.CharacterPartBusyStates.CLUMSY_MOVE_ALIGN_CHANGE && _awaitingMoveDirection.HasValue)
        {
            if (IsAbleToMove)
            {
                CharComponents.CharacterVisual.FlippedH = !CharComponents.CharacterVisual.FlippedH;
                ForceMove(_awaitingMoveDirection.Value);
            }
            _awaitingMoveDirection = null;
        }
    }

    public float GetCurrentMoveDirection()
    {
        return _currentMoveDirection;
    }
    public float GetLastMoveDirection()
    {
        return _lastMoveDirection;
    }

    public bool GetIsMaxSpeed()
    {
        return CharComponents.CharacterRigidBody.linearVelocityX > Speed - .05f || CharComponents.CharacterRigidBody.linearVelocityX < -Speed + .05f;
    }

    public void Animator_PlayStepSound()
    {
        StepSound.PlaySound();
    }
}
