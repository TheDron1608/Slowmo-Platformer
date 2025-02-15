using System;
using System.Collections;
using UnityEngine;

public class CharacterDamaging : AbstractCharacterComponent
{
    public float StunDuration = 1f; //stun duration when character is hit
    public bool ResetStunIfIsNotOnFloor = true;

    private float _stunTimeLeft = 0f;

    public bool TryApplyHit(CharacterHitbox hitLocation, AbstractProjectile projectile)
    {
        _stunTimeLeft = StunDuration;

        CharComponents.CharacterRigidBody.linearVelocity = projectile.KnockBack * VectorMath.Quartenion2DToVec2(projectile.transform.rotation);

        return true;
    }

    public bool IsStunned()
    {
        return _stunTimeLeft > 0f;
    }

    private void FixedUpdate()
    {
        if (_stunTimeLeft > 0f)
        {
            if (CharComponents.CharacterCollisionInfo.IsCollidingFloor())
            {
                _stunTimeLeft -= Time.deltaTime;
            }
            else if (ResetStunIfIsNotOnFloor)
            {
                _stunTimeLeft = StunDuration;
            }

            if (_stunTimeLeft < 0f)
            {
                _stunTimeLeft = 0f;
            }
        }
    }
}
