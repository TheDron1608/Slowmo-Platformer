using UnityEngine;

public abstract class AbstractShieldEffectWithSender : AbstractEffectWithSender, IShieldEffect
{
    private Shield _shield;

    public Shield Shield
    {
        get => _shield;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out Shield s);
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (!transform.parent.TryGetComponent(out _shield)) throw new UnityException("Shield component not found at " + gameObject.name);
    }
}
