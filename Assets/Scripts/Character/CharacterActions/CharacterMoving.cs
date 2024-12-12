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
    private CollisionCharacterInfo _collisionCharacterInfoComponent;

    private float _currentMoveDirection;
    private bool _isAbleToMoveThisFrame = true;

    public bool IsAbleToMoveThisFrame
    {
        get => _isAbleToMoveThisFrame;
        private set => _isAbleToMoveThisFrame = value;  
    }

    public float Speed = 5f;
    public float SpeedAccelerationMultiplier = 5f;

    public event EventHandler<float> OnMoveAlignChanged;
    public event EventHandler<float> OnReachedMaxSpeed;

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CharacterVisual>(out _characterVisualComponent)) throw new UnityException("CharacterVisual component not found");
        if (!TryGetComponent<CollisionCharacterInfo>(out _collisionCharacterInfoComponent)) throw new UnityException("CollisionCharacterInfo component not found");
    }

    private void Update()
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
            _rigidBodyComponent.linearVelocityX = math.lerp(_rigidBodyComponent.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationMultiplier * Time.deltaTime);
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
        if (_currentMoveDirection == direction) return;

        OnMoveAlignChanged?.Invoke(this, direction);

        _currentMoveDirection = direction;
    }

    public void Move(MoveDirection direction)
    {
        Move((int)direction);
    }

    public void MoveLeft()
    {
        Move(-1);
    }
    public void MoveRight()
    {
        Move(1);
    }

    public float GetCurrentMoveDirection()
    {
        return _currentMoveDirection;
    }

    public bool GetIsMaxSpeed()
    {
        return _rigidBodyComponent.linearVelocityX > Speed - .05f || _rigidBodyComponent.linearVelocityX < -Speed + .05f;
    }
}
