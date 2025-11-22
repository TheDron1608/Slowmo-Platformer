using UnityEngine;

public class LifeStealOnKill : AbstractDamagableObjectEffect
{
    public float LifeStealMultiplier = 1f;

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.GetComponent<IEffectApplier>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        foreach (IEffectApplier effectApplier in AffectedObject.GetComponents<IEffectApplier>())
        {
            effectApplier.OnEffectApplied += EffectApplier_OnEffectApplied;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        foreach (IEffectApplier effectApplier in AffectedObject.GetComponents<IEffectApplier>())
        {
            effectApplier.OnEffectApplied -= EffectApplier_OnEffectApplied;
        }
    }

    private void EffectApplier_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (e.Effect is ILethalEffect && e.Receiver.TryGetComponent(out AbstractCharacterComponent killedCharacter))
        {
            AffectedDamagableObject.ApplyDamage(-killedCharacter.CharComponents.CharacterHealth.MaxHealth * LifeStealMultiplier, killedCharacter);
        }
    }
}
