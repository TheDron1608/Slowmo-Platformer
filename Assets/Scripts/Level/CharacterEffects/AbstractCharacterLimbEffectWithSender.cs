using UnityEngine;

public abstract class AbstractCharacterLimbEffectWithSender : AbstractEffectWithSender, ICharacterLimbEffect
{
    private CharacterLimbPart _affectedLimbPart;

    public CharacterLimbPart AffectedLimbPart
    {
        get => _affectedLimbPart;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<CharacterLimbPart>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        _affectedLimbPart = AffectedObject.GetComponent<CharacterLimbPart>();
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && AffectedLimbPart == (other as AbstractCharacterLimbEffectWithSender).AffectedLimbPart;
    }
}
