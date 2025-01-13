using UnityEngine;

public abstract class MeleeWeapon : Weapon
{
    public float AttackRangeMultiplier = 1f;

    protected override void OnAttack()
    {
        base.OnAttack();

        if (CurrentHolder.TryGetComponent(out CharacterAiming characterAiming) && CurrentHolder.TryGetComponent(out Rigidbody2D rigidBody))
        {
            Vector2 charAimNormalized = characterAiming.GetCurrentAimNormalized();

            rigidBody.linearVelocity += charAimNormalized * KnockBack;

            if (CurrentHolder.TryGetComponent(out CharacterVisual charVisual))
            {
                charVisual.SpritesFlipped = charAimNormalized.x < 0f;
            }
        }
    }
}
