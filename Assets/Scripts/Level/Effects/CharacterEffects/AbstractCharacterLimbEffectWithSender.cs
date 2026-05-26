using UnityEngine;

public abstract class AbstractCharacterLimbEffectWithSender : AbstractEffectWithSender, ICharacterPartEffect
{
    private CharacterPart _affectedPart;

    public CharacterPart AffectedPart
    {
        get => _affectedPart;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out CharacterPart charPart);
    }

    protected override void OnApply()
    {
        base.OnApply();
        _affectedPart = AffectedObject.GetComponent<CharacterPart>();
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && AffectedPart == (other as AbstractCharacterLimbEffectWithSender)?.AffectedPart;
    }
}
