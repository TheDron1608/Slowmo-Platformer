using UnityEngine;

public class Damage : AbstractCharacterLimbEffectWithSender
{
    public float DamageAmount = 1f;

    /// <summary>
    /// warning: will delete itself after invoke this function
    /// </summary>
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AffectedPart.GetComponent<CharacterLimbPart>()?.CharPartHealth.ApplyDamage(DamageAmount, sender);

        RemoveSelf();
    }
}
