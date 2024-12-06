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

    private float _currentMoveDirection;

    public float Speed = 5f;
    public float SpeedAccelerationMultiplier = 5f;

    public event EventHandler<float> OnMoveAlignChanged;
    public event EventHandler<float> OnReachedMaxSpeed;

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CharacterVisual>(out _characterVisualComponent)) throw new UnityException("CharacterVisual component not found");
    }

    private void Update()
    {
        UpdateMoving();
    }

    private void UpdateMoving()
    {
        bool isAlreadyReachedMaxSpeed = GetIsMaxSpeed();

        _rigidBodyComponent.linearVelocityX = math.lerp(_rigidBodyComponent.linearVelocityX, _currentMoveDirection * Speed, Speed * SpeedAccelerationMultiplier * Time.deltaTime);

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
        if (_currentMoveDirection == 0f && direction != 0f)
        {
            _characterVisualComponent.MainState = CharacterPart.CharacterPartMainStates.MOVE;
            if (_characterVisualComponent.SpritesFlipped && direction > 0)
            {
                _characterVisualComponent.SpritesFlipped = false;
            }
            else if (!_characterVisualComponent.SpritesFlipped && direction < 0)
            {
                _characterVisualComponent.SpritesFlipped = true;
            }
        }
        else if (_currentMoveDirection != 0f && direction == 0f)
        {
            _characterVisualComponent.MainState = CharacterPart.CharacterPartMainStates.IDLE;
        }

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

    public bool GetIsMaxSpeed()
    {
        return _rigidBodyComponent.linearVelocityX > Speed - .05f || _rigidBodyComponent.linearVelocityX < -Speed + .05f;
    }
}
