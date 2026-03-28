using UnityEngine;

public class UnloadRangedByOwner : AbstractRangedWeaponEffect
{
    const float DROP_VELOCITY = 0.25f;

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.TryUnloadAllBullets();
        if (RangedWeapon.TryGetComponent(out Holdable holdableWeapon))
        {
            holdableWeapon.enabled = false;
            if (holdableWeapon.CurrentHolder == null)
            {
                RemoveSelf();
            }
        }
    }

    private void FixedUpdate()
    {
        if (RangedWeapon.TryGetComponent(out Holdable holdableWeapon) && holdableWeapon.CurrentHolder != null)
        {
            if (RangedWeapon.GetIsOutOfAmmo())
            {
                holdableWeapon.CurrentHolder.TryThrow(
                    new Vector2(holdableWeapon.CurrentHolder.CharComponents.CharacterVisual.FlippedH ? 1f : -1f, 1f),
                    DROP_VELOCITY
                    );

                RemoveSelf();
            }
        }
        else
        {
            RemoveSelf();
        }
    }
}
