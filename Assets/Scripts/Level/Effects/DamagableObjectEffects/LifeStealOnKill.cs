using UnityEngine;

public class LifeStealOnKill : AbstractDamagableObjectEffect, IMultiplierableEffect
{
    public float LifeStealMultiplier = 1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

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
            AffectedDamagableObject.ApplyDamage(-killedCharacter.CharComponents.CharacterHealth.MaxHealth * LifeStealMultiplier * EffectMultiplier, killedCharacter, 0f);
        }
    }
}
