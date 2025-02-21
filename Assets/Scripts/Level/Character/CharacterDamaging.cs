using System;
using UnityEngine;

public class CharacterDamaging : AbstractCharacterComponent
{
    public event EventHandler<AbstractProjectile> OnHit;

    public bool TryApplyHit(CharacterHitbox hitLocation, AbstractProjectile projectile)
    {
        Vector2 projectileDirection = VectorMath.Quartenion2DToVec2(projectile.transform.rotation);

        CharComponents.CharacterRigidBody.linearVelocity = projectile.KnockBack * projectileDirection;

        CharComponents.CharacterVisual.SpritesFlipped = CharComponents.CharacterRigidBody.linearVelocityX < 0f;

        OnHit?.Invoke(this, projectile);

        return true;
    }
}
