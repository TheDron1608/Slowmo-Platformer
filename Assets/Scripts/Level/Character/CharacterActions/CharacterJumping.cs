using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterJumping : AbstractCharacterComponent
{
    [SerializeField] private bool _isAbleToJump = true;
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
    private bool _awaitingClumsyJump = false;
    
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
        if (_isJumping && _jumpTimeLeft > 0f && _isAbleToJump)
        {
            CharComponents.CharacterRigidBody.linearVelocityY += JumpForce * JumpKeepForceMultiplier * Time.fixedDeltaTime;
        }
    }

    private void UpdateJumpLimit()
    {
        if (CharComponents.CharacterRigidBody.linearVelocityY > JumpForce)
        {
            CharComponents.CharacterRigidBody.linearVelocityY = math.lerp(
                CharComponents.CharacterRigidBody.linearVelocityY,
                JumpForce,
                Time.fixedDeltaTime * JumpLimitForceMultiplier
                );
        }
    }


    public void TryStartJump()
    {
        if (CharComponents.CharacterClumsyness.ClumsyJumping && CharComponents.CharacterCollision.IsCollidingFloor())
        {
            if (CharComponents.CharacterVisual.IsBusy()) return;

            CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.CLUMSY_JUMP_CHANGE;
            _awaitingClumsyJump = true;
            CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
        }
        else
        {
            ForceStartJump();
        }
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.OldState == CharacterVisual.CharacterPartBusyStates.CLUMSY_JUMP_CHANGE && _awaitingClumsyJump)
        {
            ForceStartJump();
        }
        CharComponents.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
    }

    public void ForceStartJump()
    {
        if (_isJumping || !IsAbleToJump) return;

        if (GetIsAbleToJumpFromFloorOrWall())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityY < JumpForce)
            {
                CharComponents.CharacterRigidBody.linearVelocityY = JumpForce;
            }

            if (CharComponents.CharacterCollision.GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY)
            {
                CharComponents.CharacterRigidBody.linearVelocityX += JumpOffWallForce;
            }
            else if (CharComponents.CharacterCollision.GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY)
            {
                CharComponents.CharacterRigidBody.linearVelocityX -= JumpOffWallForce;
            }
        }
        else if (GetIsAbleToJumpFromAir())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityY < JumpForce)
            {
                CharComponents.CharacterRigidBody.linearVelocityY = JumpForce;
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

    public void TryStartCoyoteJump()
    {
        if (CharComponents.CharacterClumsyness.ClumsyJumping && !CharComponents.CharacterCollision.IsCollidingFloor()) return;

        if (_isJumping || !IsAbleToJump) return;

        if (CharComponents.CharacterRigidBody.linearVelocityY < JumpForce)
        {
            CharComponents.CharacterRigidBody.linearVelocityY = JumpForce;
            _jumpTimeLeft = JumpMaxTime;
        }

        _isJumping = true;

        OnStartedJumping?.Invoke(this, EventArgs.Empty);
    }

    public void StopJump()
    {
        _awaitingClumsyJump = false;

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
        while (CharComponents.CharacterRigidBody.linearVelocityY > 0f)
        {
            float limitedStopJumpForce = math.lerp(CharComponents.CharacterRigidBody.linearVelocityY, 0f, Time.fixedDeltaTime);
            if (limitedStopJumpForce > JumpKeepForceMultiplier)
            {
                CharComponents.CharacterRigidBody.linearVelocityY -= JumpKeepForceMultiplier;
            }
            else
            {
                CharComponents.CharacterRigidBody.linearVelocityY = limitedStopJumpForce;
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
        return CharComponents.CharacterCollision.IsCollidingFloor();
    }

    public bool GetIsAbleToJumpFromWall()
    {
        return CharComponents.CharacterCollision.GetIsStickingOnWall();
    }

    public bool GetIsAbleToJumpFromAir()
    {
        return _airJumpsLeft > 0;
    }

    public float GetJumpHeight()
    {
        return math.pow(JumpForce, 2) / (2 * Physics2D.gravity.magnitude * CharComponents.CharacterRigidBody.gravityScale);
    }            
    
    public float GetJumpWidth()
    {
        return CharComponents.CharacterMoving.Speed * (JumpForce / (Physics2D.gravity.magnitude * CharComponents.CharacterRigidBody.gravityScale));
    }
}
