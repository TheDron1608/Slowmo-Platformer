using UnityEngine;

public class ChainsawProjectile : MeleeProjectile
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == LayerManager.ENVIROMENT_TAG_NAME && Weapon != null)
        {
            if (Weapon.TryGetComponent(out Holdable holdableWeapon) && holdableWeapon.CurrentHolder != null && holdableWeapon.CurrentHolder.TryGetComponent(out Rigidbody2D holderRigidBody))
            {
                holderRigidBody.linearVelocity -= VectorMath.Quartenion2DToVec2(transform.rotation) * WallKnockback * Time.deltaTime;
            }
            else if (Weapon.TryGetComponent(out Rigidbody2D weaponRigidBody) && weaponRigidBody.simulated)
            {
                weaponRigidBody.linearVelocity -= VectorMath.Quartenion2DToVec2(transform.rotation) * WallKnockback * Time.deltaTime;
            }
        }
    }
}
