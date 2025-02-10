using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterJumping : AbstractCharacterComponent
{
    public float JumpForce = 5f;
    public float JumpKeepForceMultiplier = 2f;
    public float JumpOffWallForce = 7.5f;
    public float JumpMaxTime = 1f;
    public bool CanForceStopJump = false;
    public int AirJumps = 0;
    public float JumpLimitForceMultiplier = 10f;

    private float _jumpTimeLeft = 0f;
    private int _airJumpsLeft = 0;
    private bool _isJumping = false;
    private bool _isAbleToJump = false;

    public event EventHandler OnStartedJumping;
    public event EventHandler OnStopedJumping;

    public bool IsAbleToJump
    {
        get => _isAbleToJump;
        set
        {
            _isAbleToJump = value;
            StopJump();
        }
    }

    public bool GetIsJumping()
    {
        return _isJumping;
    }

    private void FixedUpdate()
    {
        UpdateJumTimeLeft();
        UpdateJump();
        UpdateJumpLimit();
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
                _jumpTimeLeft -= Time.fixedDeltaTime;
            }
        }
    }

    private void UpdateJump()
    {
        if (_isJumping && _jumpTimeLeft > 0f)
        {
            _charComponents.CharacterRigidBody.linearVelocityY += JumpForce * JumpKeepForceMultiplier * Time.fixedDeltaTime;
        }
    }

    private void UpdateJumpLimit()
    {
        if (_charComponents.CharacterRigidBody.linearVelocityY > JumpForce)
        {
            _charComponents.CharacterRigidBody.linearVelocityY = math.lerp(
                _charComponents.CharacterRigidBody.linearVelocityY,
                JumpForce,
                Time.fixedDeltaTime * JumpLimitForceMultiplier
                );
        }
    }


    public void StartJump()
    {
        if (_isJumping) return;

        if (GetIsAbleToJumpFromFloorOrWall()) 
        {
            if (_charComponents.CharacterRigidBody.linearVelocityY < JumpForce)
            {
                _charComponents.CharacterRigidBody.linearVelocityY = JumpForce;
            }

            if (_charComponents.CharacterCollisionInfo.GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY)
            {
                _charComponents.CharacterRigidBody.linearVelocityX += JumpOffWallForce;
            }
            else if (_charComponents.CharacterCollisionInfo.GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY)
            {
                _charComponents.CharacterRigidBody.linearVelocityX -= JumpOffWallForce;
            }
        }
        else if (GetIsAbleToJumpFromAir())
        {
            if (_charComponents.CharacterRigidBody.linearVelocityY < JumpForce)
            {
                _charComponents.CharacterRigidBody.linearVelocityY = JumpForce;
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

        if (_charComponents.CharacterRigidBody.linearVelocityY < JumpForce)
        {
            _charComponents.CharacterRigidBody.linearVelocityY = JumpForce;
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
        while (_charComponents.CharacterRigidBody.linearVelocityY > 0f)
        {
            float limitedStopJumpForce = math.lerp(_charComponents.CharacterRigidBody.linearVelocityY, 0f, Time.fixedDeltaTime);
            if (limitedStopJumpForce > JumpKeepForceMultiplier)
            {
                _charComponents.CharacterRigidBody.linearVelocityY -= JumpKeepForceMultiplier;
            }
            else
            {
                _charComponents.CharacterRigidBody.linearVelocityY = limitedStopJumpForce;
            }

            yield return new WaitForFixedUpdate();
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
        return _charComponents.CharacterCollisionInfo.IsCollidingFloor();
    }

    public bool GetIsAbleToJumpFromWall()
    {
        return _charComponents.CharacterCollisionInfo.GetIsStickingOnWall();
    }

    public bool GetIsAbleToJumpFromAir()
    {
        return _airJumpsLeft > 0;
    }
}
