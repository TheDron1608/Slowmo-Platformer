using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class Knockback : AbstractCharacterEffectWithSender, IEntireCharacterEffect, IMultiplierableEffect
{
    public float KnockbackForce = 5f;
    public bool CanFlipSprites = true;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        float totalKnockMultiplier = EffectMultiplier;
        foreach (KnockResistance knockResistance in AffectedCharacter.CharacterEffectsReceiver.GetEffects<KnockResistance>())
        {
            totalKnockMultiplier *= knockResistance.KnockMultiplier;
        }
        if (AffectedObject.TryGetComponent(out CharacterLimbPart limbPart))
        {
            foreach (KnockLimbResistance knockResistance in limbPart.CharPartEffectsReceiver.GetEffects<KnockLimbResistance>())
            {
                totalKnockMultiplier *= knockResistance.KnockMultiplier;
            }
        }

        if (sender?.TryGetComponent(out AbstractProjectile projectile) ?? false)
        {
            AffectedCharacter.CharacterRigidBody.linearVelocity += KnockbackForce * VectorMath.Quartenion2DToVec2(projectile.transform.rotation) * totalKnockMultiplier;
        }
        else if (sender?.TryGetComponent(out Rigidbody2D rigidBody) ?? false)
        {
            AffectedCharacter.CharacterRigidBody.linearVelocity += KnockbackForce * rigidBody.linearVelocity.normalized * totalKnockMultiplier;
        }
        else
        {
            throw new UnityException("Knockback effect must be sended by GameObject containing AbstractProjectile or RigihBody2D component");
        }

        if (CanFlipSprites)
        {
            AffectedCharacter.CharacterVisual.FlippedH = AffectedCharacter.CharacterRigidBody.linearVelocityX < 0f;
        }

        RemoveSelf();
    }
}
