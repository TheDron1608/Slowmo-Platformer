using Unity.Mathematics;
using UnityEngine;

public class BlockMeleeProjectile : AbstractMeleeProjectileDeflection
{
    const float KNOCKBACK_ON_FLOOR_MIN_VELOCITY = 3f;

    public float KnockBackOnBlock = 15f;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {

        if (sender.TryGetComponent(out MeleeProjectile blockedMeleeProjectile))
        {
            blockedMeleeProjectile.IsAbleToHit = false;

            if (blockedMeleeProjectile.Owner != null)
            {
                Vector2 targetKnockback;
                if (MeleeProjectile != null)
                {
                    targetKnockback = VectorMath.Quartenion2DToVec2(MeleeProjectile.transform.rotation) * KnockBackOnBlock;
                }
                else
                {
                    targetKnockback = -VectorMath.Quartenion2DToVec2(blockedMeleeProjectile.transform.rotation) * KnockBackOnBlock;
                }

                if (blockedMeleeProjectile.Owner.CharComponents.CharacterCollision.IsCollidingFloor())
                {
                    targetKnockback.y = math.max(KNOCKBACK_ON_FLOOR_MIN_VELOCITY, targetKnockback.y);
                }

                if (
                    !(targetKnockback.x > 0f ^ blockedMeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity.x > 0f) &&
                    !(targetKnockback.y > 0f ^ blockedMeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity.y > 0f)
                    )
                {
                    blockedMeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity += targetKnockback;
                }
                else
                {
                    blockedMeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity = targetKnockback;
                }

                blockedMeleeProjectile?.Owner?.CharComponents.CharacterEffectsReceiver.ApplyEffect(MeleeProjectile.EffectsOnDeflect, MeleeProjectile);
            }
        }

        RemoveSelf();
    }
}
