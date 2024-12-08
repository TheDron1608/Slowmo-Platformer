using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterJumping : MonoBehaviour
{
    public float JumpForce = 5f;
    public float JumpKeepForceMultiplier = 2f;
    public float JumpMaxTime = 1f;
    public int AirJumps = 0;

    private float _jumpTimeLeft = 0f;
    private int _airJumpsLeft = 0;
    private bool _isJumping = false;

    private Rigidbody2D _rigidBodyComponent;
    private CharacterInfo _characterInfoComponent;

    public event EventHandler OnStartedJumping;
    public event EventHandler OnStopedJumping;

    public bool GetIsJumping()
    {
        return _isJumping;
    }

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CharacterInfo>(out _characterInfoComponent)) throw new UnityException("CharacterInfo component not found");
    }

    private void Update()
    {
        if (_characterInfoComponent.IsCollidingFloor())
        {
            _jumpTimeLeft = JumpMaxTime;
            _airJumpsLeft = AirJumps;
        }
        else
        {
            if (_jumpTimeLeft < 0f)
            {
                _jumpTimeLeft = 0f;
                OnStopedJumping?.Invoke(this, EventArgs.Empty);
                _isJumping = false;
            }
            else
            {
                _jumpTimeLeft -= Time.deltaTime;
            }
        }

        if (_isJumping && _jumpTimeLeft > 0f)
        {
            _rigidBodyComponent.linearVelocityY += JumpForce * JumpKeepForceMultiplier * Time.deltaTime;
        }
    }

    public void StartJump()
    {
        if (_isJumping) return;

        if (_characterInfoComponent.IsCollidingFloor()) 
        {
            _rigidBodyComponent.linearVelocityY = JumpForce;
        }
        else if (_airJumpsLeft > 0)
        {
            if (_rigidBodyComponent.linearVelocityY < JumpForce)
            {
                _rigidBodyComponent.linearVelocityY = JumpForce;
                _jumpTimeLeft = JumpMaxTime;
            }

            _airJumpsLeft--;
            if (_airJumpsLeft < 0)
            {
                _airJumpsLeft = 0;
            }
        }


        _isJumping = true;

        OnStartedJumping?.Invoke(this, EventArgs.Empty);
    }

    public void ForceStartJump()
    {
        if (_isJumping) return;

        if (_rigidBodyComponent.linearVelocityY < JumpForce)
        {
            _rigidBodyComponent.linearVelocityY = JumpForce;
            _jumpTimeLeft = JumpMaxTime;
        }

        _isJumping = true;

        OnStartedJumping?.Invoke(this, EventArgs.Empty);
    }

    public void StopJump()
    {
        if (!_isJumping) return;

        _isJumping = false;

        OnStopedJumping?.Invoke(this, EventArgs.Empty);
    }

    public bool GetIsAbleToJump()
    {
        return _characterInfoComponent.IsCollidingFloor() || _airJumpsLeft > 0;
    }


    public bool GetIsAbleToJumpFromFloor()
    {
        return _characterInfoComponent.IsCollidingFloor();
    }
}
