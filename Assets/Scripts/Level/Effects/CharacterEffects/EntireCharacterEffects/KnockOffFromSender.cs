using UnityEngine;

public class KnockOffFromSender : AbstractCharacterEffectWithSender, IEntireCharacterEffect, IMultiplierableEffect
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

        AffectedCharacter.CharacterRigidBody.linearVelocity += KnockbackForce * VectorMath.Vec3ToVec2((AffectedCharacter.Center.transform.position - sender.transform.position).normalized);

        if (CanFlipSprites)
        {
            AffectedCharacter.CharacterVisual.FlippedH = AffectedCharacter.CharacterRigidBody.linearVelocityX < 0f;
        }

        RemoveSelf();
    }
}
