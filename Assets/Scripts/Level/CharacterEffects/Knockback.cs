using UnityEngine;

public class Knockback : AbstractCharacterEffectWithSender
{
    public float KnockbackForce = 5f;
    public bool CanFlipSprites = true;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPartHealth receiverPart)
    {
        if (sender.TryGetComponent(out AbstractProjectile projectile))
        {
            AffectedCharacter.CharacterRigidBody.linearVelocity += KnockbackForce * VectorMath.Quartenion2DToVec2(projectile.transform.rotation);
        }
        else if (sender.TryGetComponent(out Rigidbody2D rigidBody))
        {
            AffectedCharacter.CharacterRigidBody.linearVelocity += KnockbackForce * rigidBody.linearVelocity.normalized;
        }
        else
        {
            throw new UnityException("Knockback effect must be sended by GameObject containing AbstractProjectile component");
        }

        if (CanFlipSprites)
        {
            AffectedCharacter.CharacterVisual.FlippedH = AffectedCharacter.CharacterRigidBody.linearVelocityX < 0f;
        }

        RemoveSelf();
    }
}
