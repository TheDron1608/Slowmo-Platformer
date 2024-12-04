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

    public float Speed = 5f;
    public float SpeedAccelerationMultiplier = 5f;

    public event EventHandler<float> OnMoveAlignChanged;
    public event EventHandler<float> OnReachedMaxSpeed;

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
    }

    /// <summary>
    /// Character moves horizontally with "align" speed
    /// </summary>
    /// <param name="direction">Value between -1 and 1</param>
    public void Move(float direction)
    {
        bool isAlreadyReachedMaxSpeed = GetIsMaxSped();

        _rigidBodyComponent.linearVelocityX = math.lerp(_rigidBodyComponent.linearVelocityX, direction * Speed, Speed * SpeedAccelerationMultiplier * Time.deltaTime);

        OnMoveAlignChanged?.Invoke(this, direction);

        if (!isAlreadyReachedMaxSpeed && GetIsMaxSped())
        {
            OnReachedMaxSpeed?.Invoke(this, direction);
        }
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

    public bool GetIsMaxSped()
    {
        return _rigidBodyComponent.linearVelocityX > Speed - .05f || _rigidBodyComponent.linearVelocityX < -Speed + .05f;
    }
}
