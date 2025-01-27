using System;
using Unity.Mathematics;
using UnityEngine;

public class CharacterMoving : MonoBehaviour
{
    public enum MoveDirection
    {
        None = 0,
        Left = -1,
        Right = 1
    }

    private Rigidbody2D _rigidBodyComponent;
    private CharacterVisual _characterVisualComponent;
    private CharacterCollisionInfo _collisionCharacterInfoComponent;

    private float _currentMoveDirection;
    private bool _isAbleToMoveThisFrame = true;
    private bool _isAbleToMove = true;
    private float _lastMoveDirectrion = 0f;

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
                Move(_lastMoveDirectrion);
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

    public event EventHandler<float> OnMoveAlignChanged;
    public event EventHandler<float> OnReachedMaxSpeed;

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _characterVisualComponent)) throw new UnityException("CharacterVisual component not found");
        if (!TryGetComponent(out _collisionCharacterInfoComponent)) throw new UnityException("CollisionCharacterInfo component not found");
    }

    private void FixedUpdate()
    {
        UpdateMoving();
    }

    private void UpdateMoving()
    {
        bool isAlreadyReachedMaxSpeed = GetIsMaxSpeed();

        if (_currentMoveDirection > 0f && _collisionCharacterInfoComponent.IsCollidingRightWall())
        {
            if (_rigidBodyComponent.linearVelocityX > 0) _rigidBodyComponent.linearVelocityX = 0f;
            _isAbleToMoveThisFrame = false;
            OnMoveAlignChanged?.Invoke(this, 0f);
        }
        else if (_currentMoveDirection < 0f && _collisionCharacterInfoComponent.IsCollidingLeftWall())
        {
            if (_rigidBodyComponent.linearVelocityX < 0) _rigidBodyComponent.linearVelocityX = 0f;
            _isAbleToMoveThisFrame = false;
            OnMoveAlignChanged?.Invoke(this, 0f);
        }
        else
        {
            _isAbleToMoveThisFrame = true;
            if (_collisionCharacterInfoComponent.IsCollidingFloor())
            {
                _rigidBodyComponent.linearVelocityX = math.lerp(_rigidBodyComponent.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationOnGroundMultiplier * Time.fixedDeltaTime);
            }
            else
            {
                _rigidBodyComponent.linearVelocityX = math.lerp(_rigidBodyComponent.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationOnAirMulitplier * Time.fixedDeltaTime);
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
    public void Move(float direction)
    {
        _lastMoveDirectrion = direction;

        if (!_isAbleToMove) return;
        if (_currentMoveDirection == direction && _lastMoveDirectrion == direction) return;

        OnMoveAlignChanged?.Invoke(this, direction);

        _currentMoveDirection = direction;
    }

    public void Move(MoveDirection direction)
    {
        Move((int)direction);
    }

    public void MoveLeft()
    {
        Move(-1f);
    }
    public void MoveRight()
    {
        Move(1f);
    }
    public void StopMove()
    {
        Move(0f);
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
        return _rigidBodyComponent.linearVelocityX > Speed - .05f || _rigidBodyComponent.linearVelocityX < -Speed + .05f;
    }
}
