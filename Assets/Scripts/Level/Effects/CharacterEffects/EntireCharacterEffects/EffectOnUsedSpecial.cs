using Unity.VisualScripting;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class EffectOnUsedSpecial : AbstractCharacterEffectWithSender, IEntireCharacterEffect, IMultiplierableEffect
{
    public AbstractEffect EffectOnAbility;

    private float _effectMultiplier = 1f;
    private AbstractCharacterSpecial _currentSpecial = null;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (_currentSpecial == null || _currentSpecial.IsDestroyed())
        {
            if (AffectedCharacter.CharacterSpecial != null)
            {
                _currentSpecial = AffectedCharacter.CharacterSpecial;
                _currentSpecial.OnUsed += CharacterSpecial_OnUsed;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_currentSpecial == null || _currentSpecial.IsDestroyed())
        {
            if (AffectedCharacter.CharacterSpecial != null)
            {
                _currentSpecial = AffectedCharacter.CharacterSpecial;
                _currentSpecial.OnUsed += CharacterSpecial_OnUsed;
            }
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (_currentSpecial != null)
        {
            _currentSpecial.OnUsed -= CharacterSpecial_OnUsed;
        }
    }

    private void CharacterSpecial_OnUsed(object sender, System.EventArgs e)
    {
        if (SpecialCondition(_currentSpecial))
        {
            AffectedCharacter.CharacterEffectsReceiver.ApplyEffect(EffectOnAbility, Sender, EffectMultiplier);
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out AbstractCharacterComponent character);
    }

    protected virtual bool SpecialCondition(AbstractCharacterSpecial special)
    {
        return special != null;
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            (EffectOnAbility?.Equals((other as EffectOnUsedSpecial).EffectOnAbility) ?? (other as EffectOnUsedSpecial).EffectOnAbility == EffectOnAbility);
    }
}
