using UnityEngine;

public class KnockWeapon : AbstractWeaponEffect
{
    public float KnockbackForce = 5f;
    public Vector2 KnockbackDirection = new();
    public float KnockbackAngularForce = 0f;

    protected override void OnApply()
    {
        base.OnApply();

        Rigidbody2D weaponRigidBody = Weapon.GetComponent<Rigidbody2D>();
        Holdable holdableWeapon = Weapon.GetComponent<Holdable>();

        holdableWeapon.StuckedToCollider = null;

        weaponRigidBody.linearVelocity += KnockbackDirection.normalized * KnockbackForce;
        weaponRigidBody.angularVelocity += (NumberMath.RandomCoinflip() ? 1f : -1f) * KnockbackAngularForce;

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out Holdable holdableWeapon) &&
            holdableWeapon.CurrentHolder == null &&
            affectWho.TryGetComponent(out Rigidbody2D rb);
    }
}
