using UnityEngine;

public class SetUnlimitedHealth : AbstractDamagableObjectEffect
{
    public bool Value = true;

    private bool _oldValue = false;

    protected override void OnApply()
    {
        base.OnApply();

        _oldValue = AffectedDamagableObject.UnlimitedHealth;
        AffectedDamagableObject.UnlimitedHealth = Value;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedDamagableObject.UnlimitedHealth = _oldValue;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<SetUnlimitedHealth>();
    }

    public override bool Equals(AbstractEffect other)
    {
        return 
            base.Equals(other) &&
            Value == (other as SetUnlimitedHealth).Value;
    }
}
