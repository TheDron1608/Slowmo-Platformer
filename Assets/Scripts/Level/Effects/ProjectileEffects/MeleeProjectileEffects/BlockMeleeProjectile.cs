using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BlockMeleeProjectile : AbstractMeleeProjectileDeflection
{
    const float KNOCKBACK_ON_FLOOR_MIN_VELOCITY = 3f;

    public float KnockBackOnBlock = 15f;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {

        MeleeProjectile.IsAbleToHit = false;

        if (MeleeProjectile.Owner != null)
        {
            Vector2 targetKnockback;
            if (MeleeProjectile != null)
            {
                targetKnockback = -VectorMath.Quartenion2DToVec2(MeleeProjectile.transform.rotation) * KnockBackOnBlock;
            }
            else
            {
                targetKnockback = VectorMath.Quartenion2DToVec2(MeleeProjectile.transform.rotation) * KnockBackOnBlock;
            }

            if (MeleeProjectile.Owner.CharComponents.CharacterCollision.IsCollidingFloor())
            {
                targetKnockback.y = math.max(KNOCKBACK_ON_FLOOR_MIN_VELOCITY, targetKnockback.y);
            }

            if (
                !(targetKnockback.x > 0f ^ MeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity.x > 0f) &&
                !(targetKnockback.y > 0f ^ MeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity.y > 0f)
                )
            {
                MeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity += targetKnockback;
            }
            else
            {
                MeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity = targetKnockback;
            }

            if (MeleeProjectile != null && !MeleeProjectile.IsDestroyed())
            {
                MeleeProjectile?.Owner?.CharComponents.CharacterEffectsReceiver.ApplyEffect(MeleeProjectile.EffectsOnDeflect, MeleeProjectile);
            }
        }

        RemoveSelf();
    }
}
