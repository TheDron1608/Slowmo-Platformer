using UnityEngine;

public class AbstractDamagableObjectEffect : AbstractEffect, IDamagableObjectEffect
{
    private IDamagable _affectedDamagableObject;

    public IDamagable AffectedDamagableObject
    {
        get => _affectedDamagableObject;
        private set => _affectedDamagableObject = value;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<IDamagable>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        AffectedDamagableObject = AffectedObject.GetComponent<IDamagable>();
    }
}
