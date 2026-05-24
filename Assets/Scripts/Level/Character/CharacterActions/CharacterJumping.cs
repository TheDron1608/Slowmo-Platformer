using System;
using Unity.Mathematics;
using UnityEngine;

public class CharacterJumping : AbstractCharacterComponent
{
    const float AIR_JUMP_PARTICLE_VELOCITY = 0.5f;

    [SerializeField] private bool _isAbleToJump = true;
    public float JumpForce = 5f;
    public float KeepJumpGravityMultiplier = 0.5f;
    public float StopJumpGravityMultiplier = 4f;
    public float JumpOffWallForce = 7.5f;
    public int AirJumps = 0;
    public float JumpLimitForceMultiplier = 10f;
    public bool CanForceStopRollingOnJump = false;
    public AbstractParticle ParticleOnAirJump;
    public AbstractSoundPlayer SoundOnJump;

    private int _airJumpsLeft = 0;
    private bool _isJumping = false;
    private bool _awaitingClumsyJump = false;
    private float _currentGravityMultiplier = 1f;

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

    public bool GetIsAirJumping()
    {
        return _isJumping && _airJumpsLeft < AirJumps;
    }

    public float GetBaseGravity()
    {
        return CharComponents.CharacterRigidBody.gravityScale / _currentGravityMultiplier;
    }

    private void FixedUpdate()
    {
        if (_isJumping && !CharComponents.CharacterCollision.IsCollidingFloor())
        {
            CharComponents.CharacterRigidBody.gravityScale *= KeepJumpGravityMultiplier / _currentGravityMultiplier;
            _currentGravityMultiplier = KeepJumpGravityMultiplier;
        }
        else if (CharComponents.CharacterRigidBody.linearVelocityY > 0f && !CharComponents.CharacterCollision.IsCollidingFloor())
        {
            CharComponents.CharacterRigidBody.gravityScale *= StopJumpGravityMultiplier / _currentGravityMultiplier;
            _currentGravityMultiplier = StopJumpGravityMultiplier;
        }
        else
        {
            CharComponents.CharacterRigidBody.gravityScale *= 1 / _currentGravityMultiplier;
            _currentGravityMultiplier = 1;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _airJumpsLeft = AirJumps;
    }

    private void OnEnable()
    {
        StopJump();
    }

    public void TryStartJump()
    {
        if (
            (
                (
                    CharComponents.CharacterVisual.IsBusy() && 
                    !(CanForceStopRollingOnJump && CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.ROLL) &&
                    !CharComponents.CharacterVisual.AllowMovementOnBusyAnimation
                    ) &&
                CharComponents.CharacterVisual.CurrentBusyAnimation != CharacterVisual.CharacterPartBusyStates.CLUMSY_JUMP_CHANGE
            ) ||
            !IsAbleToJump
            )
        {
            return;
        }

        if (CanForceStopRollingOnJump)
        {
            CharComponents.CharacterRolling.ForceStopRolling();
        }

        if (CharComponents.CharacterClumsyness.ClumsyJumping && CharComponents.CharacterCollision.IsCollidingFloor())
        {
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
            _awaitingClumsyJump = false;
            if (IsAbleToJump)
            {
                ForceStartJump();
            }
        }
        CharComponents.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
    }

    public void ForceStartJump()
    {
        if (!IsAbleToJump) return;

        if (GetIsAbleToJumpFromFloorOrWall())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityY < JumpForce)
            {
                CharComponents.CharacterRigidBody.linearVelocityY = JumpForce;
            }

            if (CharComponents.CharacterCollision.GetTileBehaviourTypeFromLeftWall() == ForegroundRuleTile.ForegroundBehaviourType.STICKY)
            {
                CharComponents.CharacterRigidBody.linearVelocityX += JumpOffWallForce;
            }
            else if (CharComponents.CharacterCollision.GetTileBehaviourTypeFromRightWall() == ForegroundRuleTile.ForegroundBehaviourType.STICKY)
            {
                CharComponents.CharacterRigidBody.linearVelocityX -= JumpOffWallForce;
            }
        }
        else if (GetIsAbleToJumpFromAir())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityY < JumpForce)
            {
                CharComponents.CharacterRigidBody.linearVelocityY = JumpForce;
            }

            if (ParticleOnAirJump != null)
            {
                ParticleSpawner.SpawnParticle(
                    ParticleOnAirJump,
                    CharComponents.Bottom.transform.position,
                    new Vector2(CharComponents.CharacterMoving.GetCurrentMoveDirection(), -1f).normalized,
                    0f,
                    AIR_JUMP_PARTICLE_VELOCITY,
                    0f,
                    CharComponents.CharacterEffectsReceiver.EffectMaterial,
                    CharComponents.CharacterCollision.CurrentZLayer
                    );
            }

            _airJumpsLeft--;
            if (_airJumpsLeft < 0)
            {
                _airJumpsLeft = 0;
            }
        }

        _isJumping = true;
        CharComponents.CharacterCollision.OnCollisionChanged += CharacterCollision_OnCollisionChanged;

        OnStartedJumping?.Invoke(this, EventArgs.Empty);
    }

    private void CharacterCollision_OnCollisionChanged(object sender, CharacterCollision.OnCollisionChangedEventArgs e)
    {
        if (e.CollisionAlign == Vector2.down && e.EnterOrReleasedCollision)
        {
            StopJump();
            _airJumpsLeft = AirJumps;
            CharComponents.CharacterCollision.OnCollisionChanged -= CharacterCollision_OnCollisionChanged;
        }
    }

    public void TryStartCoyoteJump()
    {
        if (CharComponents.CharacterClumsyness.ClumsyJumping && !CharComponents.CharacterCollision.IsCollidingFloor()) return;

        if (_isJumping || !IsAbleToJump) return;

        if (CharComponents.CharacterRigidBody.linearVelocityY < JumpForce)
        {
            CharComponents.CharacterRigidBody.linearVelocityY = JumpForce;
        }

        _isJumping = true;

        OnStartedJumping?.Invoke(this, EventArgs.Empty);
    }

    public void StopJump()
    {
        _awaitingClumsyJump = false;

        if (!_isJumping) return;

        _isJumping = false;

        OnStopedJumping?.Invoke(this, EventArgs.Empty);
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
        return !_isJumping && CharComponents.CharacterCollision.IsCollidingFloor();
    }

    public bool GetIsAbleToJumpFromWall()
    {
        return !_isJumping && CharComponents.CharacterCollision.GetIsStickingOnWall();
    }

    public bool GetIsAbleToJumpFromAir()
    {
        return _airJumpsLeft > 0;
    }

    public int GetJumpHeight()
    {

        return (int)math.ceil(math.pow(JumpForce, 2) / (2 * (Physics2D.gravity.magnitude * GetBaseGravity() * CharComponents.CharacterJumping.KeepJumpGravityMultiplier)));
    }

    public int GetJumpWidth()
    {
        return (int)math.round(CharComponents.CharacterMoving.Speed * (JumpForce / (Physics2D.gravity.magnitude * GetBaseGravity() * CharComponents.CharacterJumping.KeepJumpGravityMultiplier)));
    }
}
