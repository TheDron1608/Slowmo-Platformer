using UnityEngine;

public class MeleeWeapon : Weapon
{
    const string ANIMATOR_IS_THROWN_PROP_NAME = "IsThrown";
    const string ANIMATOR_ATTACK_TRIGGER_NAME = "Attack";

    [Header("Melee weapon")]
    public float AttackRangeMultiplier = 1f;
    public Projectile Projectile;

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

    protected override bool OnTryAttack()
    {
        _animator.SetTrigger(ANIMATOR_ATTACK_TRIGGER_NAME);

        Projectile projectile = Instantiate(Projectile, transform);

        if (CurrentHolder.TryGetComponent(out CharacterAiming characterAiming))
        {
            projectile.transform.LookAt(characterAiming.CurrentAimPoint);
            projectile.transform.rotation = VectorMath.Vec2ToQuarterninon2D(characterAiming.GetCurrentAimNormalized());
        }

        if (CurrentHolder.TryGetComponent(out Rigidbody2D rigidBody))
        {
            Vector2 charAimNormalized = characterAiming.GetCurrentAimNormalized();

            rigidBody.linearVelocity += charAimNormalized * KnockBack;

            if (CurrentHolder.TryGetComponent(out CharacterVisual charVisual))
            {
                charVisual.SpritesFlipped = charAimNormalized.x < 0f;
            }
        }

        return true;
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
