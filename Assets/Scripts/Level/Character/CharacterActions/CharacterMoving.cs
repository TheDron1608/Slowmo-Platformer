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
    private bool _isAbleToMoveThisFrame = true;
    [SerializeField] private bool _isAbleToMove = true;
    private float _lastMoveDirectrion = 0f;
    private float? _awaitingMoveDirection = null;

    public bool IsAbleToMoveThisFrame
    {
        get => _isAbleToMoveThisFrame;
        private set => _isAbleToMoveThisFrame = value;  
    }
    public bool IsAbleToMove
    {
        get => _isAbleToMove;
        set
        {
            _isAbleToMove = value;

            if (value)
            {
                TryMove(_lastMoveDirectrion);
            }
            else
            {
                _currentMoveDirection = 0f;
                OnMoveAlignChanged?.Invoke(this, _currentMoveDirection);
            }
        }
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
    }

    public bool IsMoving()
    {
        return Math.Abs(CharComponents.CharacterMoving.GetCurrentMoveDirection()) < 0.05f;
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
            _isAbleToMoveThisFrame = false;
            OnMoveAlignChanged?.Invoke(this, 0f);
        }
        else if (_currentMoveDirection < 0f && CharComponents.CharacterCollision.IsCollidingLeftWall())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityX < 0) CharComponents.CharacterRigidBody.linearVelocityX = 0f;
            _isAbleToMoveThisFrame = false;
            OnMoveAlignChanged?.Invoke(this, 0f);
        }
        else
        {
            _isAbleToMoveThisFrame = true;
            if (!IsAbleToMove)
            {
                CharComponents.CharacterRigidBody.linearVelocityX = math.lerp(CharComponents.CharacterRigidBody.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationOnUnableToMoveMultiplier * Time.fixedDeltaTime);
            }
            else if (CharComponents.CharacterCollision.IsCollidingFloor())
            {
                CharComponents.CharacterRigidBody.linearVelocityX = math.lerp(CharComponents.CharacterRigidBody.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationOnGroundMultiplier * Time.fixedDeltaTime);
            }
            else
            {
                CharComponents.CharacterRigidBody.linearVelocityX = math.lerp(CharComponents.CharacterRigidBody.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationOnAirMulitplier * Time.fixedDeltaTime);
            }
        }

        if (!isAlreadyReachedMaxSpeed && GetIsMaxSpeed())
        {
            OnReachedMaxSpeed?.Invoke(this, _currentMoveDirection);
        }
    }

    /// <summary>
    /// Character moves horizontally with "align" speed
    /// </summary>
    /// <param name="direction">Value between -1 and 1</param>
    public void TryMove(float direction)
    {
        if (_currentMoveDirection == direction || _awaitingMoveDirection == direction || CharComponents.CharacterVisual.IsBusy()) return;

        if (GetIsNeedChangeClumsyDirection(direction))
        {
            TrySetClumsyAlign(direction, false);
        }
        else
        {
            ForceMove(direction);
        }
    }

    public void TryMove(MoveDirection direction)
    {
        TryMove((int)direction);
    }

    public void ForceMove(float direction)
    {
        _lastMoveDirectrion = direction;

        if (!_isAbleToMove) return;
        if (_currentMoveDirection == direction && _lastMoveDirectrion == direction) return;

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
        return CharComponents.CharacterClumsyness.ClumsyMovement && (CharComponents.CharacterVisual.FlippedH ^ direction < 0f) && direction != 0f && !CharComponents.CharacterVisual.IsBusy();
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.NewState != CharacterVisual.CharacterPartBusyStates.NONE)
        {
            ForceMove(0f);
        }
        else if (e.OldState == CharacterVisual.CharacterPartBusyStates.CLUMSY_MOVE_ALIGN_CHANGE && _awaitingMoveDirection.HasValue)
        {
            CharComponents.CharacterVisual.FlippedH = !CharComponents.CharacterVisual.FlippedH;
            ForceMove(_awaitingMoveDirection.Value);
            _awaitingMoveDirection = null;
        }
    }

    public float GetCurrentMoveDirection()
    {
        return _currentMoveDirection;
    }
    public float GetLastMoveDirection()
    {
        return _lastMoveDirectrion;
    }

    public bool GetIsMaxSpeed()
    {
        return CharComponents.CharacterRigidBody.linearVelocityX > Speed - .05f || CharComponents.CharacterRigidBody.linearVelocityX < -Speed + .05f;
    }
}
