using UnityEngine;

public abstract class AbstractCharacterLimbEffect : AbstractEffect, ICharacterPartEffect
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
            affectWho.GetComponent<CharacterPart>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        _affectedPart = AffectedObject.GetComponent<CharacterPart>();
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) && 
            (
                AffectedPart == (other as AbstractCharacterLimbEffect).AffectedPart ||
                AffectedPart == null ||
                (other as AbstractCharacterLimbEffect).AffectedPart == null
            );
    }
}
