using UnityEngine;

public class PierceArmor : AbstractCharacterLimbEffectWithSender
{
    public LimbArmor.ArmorPierceResistantLevels PierceLevel;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (
            AffectedLimbPart.CharComponents.CharacterEffectsReceiver.TryGetEffect(out LimbArmor armorEffect, AffectedLimbPart) && 
            PierceLevel >= armorEffect.ArmorPierceResistantLevel
            )
        {
            AffectedLimbPart.CharPartEffectsReceiver.RemoveEffect<LimbArmor>();
            if (armorEffect.Sender is CharacterPart armorPart)
            {
                armorPart.DestroyPart();
            }
            else
            {
                throw new UnityException("sender of armor effect must be CharacterPart class, received " + armorEffect.Sender.GetType().Name + " instead");
            }
        }

        RemoveSelf();
    }
}
