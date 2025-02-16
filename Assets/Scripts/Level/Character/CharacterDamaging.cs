using System;
using System.Collections;
using UnityEngine;
using static CharacterVisual;

public class CharacterDamaging : AbstractCharacterComponent
{
    public float HardStunRecoverDuration = 1f;
    public bool ResetStunIfIsNotOnFloor = true;

    private float _hardStunRecoverTimeLeft = 0f;
    private AbstractProjectile.StunTypes _currentAppliedStunType = AbstractProjectile.StunTypes.NO_STUN;

    public event EventHandler<AbstractProjectile> OnHit;

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
    }

    public AbstractProjectile.StunTypes CurrentAppliedStunType
    {
        get => _currentAppliedStunType;
        private set
        {
            CharComponents.SetIsAbleToDoAnyActions(value == AbstractProjectile.StunTypes.NO_STUN);
            _currentAppliedStunType = value;
        }
    }

    public bool IsStunned()
    {
        return _currentAppliedStunType != AbstractProjectile.StunTypes.NO_STUN;
    }

    public bool IsHardStunned()
    {
        return 
            CurrentAppliedStunType == AbstractProjectile.StunTypes.HARD_STUN ||
            CurrentAppliedStunType == AbstractProjectile.StunTypes.PIERCING_HARD_STUN;
    }

    public bool IsMinorStunned()
    {
        return CurrentAppliedStunType == AbstractProjectile.StunTypes.MINOR_STUN;
    }

    public bool TryApplyHit(CharacterHitbox hitLocation, AbstractProjectile projectile)
    {
        Vector2 projectileDirection = VectorMath.Quartenion2DToVec2(projectile.transform.rotation);

        CharComponents.CharacterRigidBody.linearVelocity = projectile.KnockBack * projectileDirection;

        CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterPart.CharacterPartBusyStates.FALLING_IN_AIR;
        if (CharComponents.CharacterVisual.IsBusy())
        {
            CharComponents.CharacterVisual.BreakBusyAnimation();
        }
        CharComponents.CharacterVisual.SpritesFlipped = CharComponents.CharacterRigidBody.linearVelocityX < 0f;

        CurrentAppliedStunType = projectile.StunType;

        if (IsHardStunned())
        {
            _hardStunRecoverTimeLeft = HardStunRecoverDuration;
        }

        OnHit?.Invoke(this, projectile);

        return true;
    }

    public void Unstun()
    {
        CurrentAppliedStunType = AbstractProjectile.StunTypes.NO_STUN;
    }

    private void FixedUpdate()
    {
        if (_hardStunRecoverTimeLeft > 0f)
        {
            if (CharComponents.CharacterCollisionInfo.IsCollidingFloor())
            {
                _hardStunRecoverTimeLeft -= Time.deltaTime;
            }
            else if (ResetStunIfIsNotOnFloor)
            {
                _hardStunRecoverTimeLeft = HardStunRecoverDuration;
            }

            if (_hardStunRecoverTimeLeft < 0f)
            {
                _hardStunRecoverTimeLeft = 0f;
            }
        }
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, OnBusyStateChangedEventArgs e)
    {
        if (
            (e.OldState == CharacterPart.CharacterPartBusyStates.MINOR_STUN || e.OldState == CharacterPart.CharacterPartBusyStates.FALLEN_ON_FLOOR) &&
            e.NewState == CharacterPart.CharacterPartBusyStates.NONE
            )
        {
            CurrentAppliedStunType = AbstractProjectile.StunTypes.NO_STUN;
        }
    }
}
