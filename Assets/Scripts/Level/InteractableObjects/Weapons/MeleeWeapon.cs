using UnityEngine;

public class MeleeWeapon : Weapon
{
    const string ANIMATOR_IS_THROWN_PROP_NAME = "IsThrown";

    public float AttackRangeMultiplier = 1f;

    private bool _isThrown = true;

    public bool IsThrown
    {
        get => _isThrown;
        set
        {
            _animator.SetBool(ANIMATOR_IS_THROWN_PROP_NAME, value);
            _isThrown = value;
        }
    }

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

    protected override void OnThrow()
    {
        base.OnThrow();
        IsThrown = true;
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();
        IsThrown = false;
    }
}
