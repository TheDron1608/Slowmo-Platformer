using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterJumping : MonoBehaviour
{
    public float JumpForce = 5f;
    public float JumpKeepForceMultiplier = 2f;
    public float JumpMaxTime = 1f;
    public bool CanForceStopJump = false;
    public int AirJumps = 0;

    private float _jumpTimeLeft = 0f;
    private int _airJumpsLeft = 0;
    private bool _isJumping = false;

    private Rigidbody2D _rigidBodyComponent;
    private CollisionCharacterInfo _collisionCharacterInfoComponent;
    private CharacterInteractionWithTiles _characterInteractionWithTilesComponent;

    public event EventHandler OnStartedJumping;
    public event EventHandler OnStopedJumping;

    public bool GetIsJumping()
    {
        return _isJumping;
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _collisionCharacterInfoComponent)) throw new UnityException("CollisionCharacterInfo component not found");
        if (!TryGetComponent(out _characterInteractionWithTilesComponent)) throw new UnityException("CharacterInteractionWithTiles component not found");
    }

    private void Update()
    {
        UpdateJumTimeLeft();
        UpdateJump();
    }

    private void UpdateJumTimeLeft()
    {
        if (GetIsAbleToJumpFromFloorOrWall())
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
    }

    private void UpdateJump()
    {
        if (_isJumping && _jumpTimeLeft > 0f)
        {
            _rigidBodyComponent.linearVelocityY += JumpForce * JumpKeepForceMultiplier * Time.deltaTime;
        }
    }



    public void StartJump()
    {
        if (_isJumping) return;

        if (GetIsAbleToJumpFromFloorOrWall()) 
        {
            if (_rigidBodyComponent.linearVelocityY < JumpForce)
            {
                _rigidBodyComponent.linearVelocityY = JumpForce;
            }
        }
        else if (GetIsAbleToJumpFromAir())
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

        if (CanForceStopJump)
        {
            StartCoroutine(ForceStopJumpProcess());
        }

        OnStopedJumping?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator ForceStopJumpProcess()
    {
        while (_rigidBodyComponent.linearVelocityY > 0f)
        {
            float limitedStopJumpForce = math.lerp(_rigidBodyComponent.linearVelocityY, 0f, Time.deltaTime);
            if (limitedStopJumpForce > JumpKeepForceMultiplier)
            {
                _rigidBodyComponent.linearVelocityY -= JumpKeepForceMultiplier;
            }
            else
            {
                _rigidBodyComponent.linearVelocityY = limitedStopJumpForce;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public bool GetIsAbleToJump()
    {
        return
            GetIsAbleToJumpFromFloor() ||
            GetIsAbleToJumpFromWall() ||
            GetIsAbleToJumpFromAir();
    }

    public bool GetIsAbleToJumpFromFloorOrWall()
    {
        return
            GetIsAbleToJumpFromFloor() ||
            GetIsAbleToJumpFromWall();
    }

    public bool GetIsAbleToJumpFromFloor()
    {
        return _collisionCharacterInfoComponent.IsCollidingFloor();
    }

    public bool GetIsAbleToJumpFromWall()
    {
        return _collisionCharacterInfoComponent.GetIsStickingOnWall();
    }

    public bool GetIsAbleToJumpFromAir()
    {
        return _airJumpsLeft > 0;
    }
}
