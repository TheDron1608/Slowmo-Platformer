using Unity.VisualScripting;
using UnityEngine;

public class BreakExpludeHoldedObjects : Break
{
    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            (!affectWho.TryGetComponent(out Holdable holdable) || holdable.CurrentHolder == null || holdable.CurrentHolder.IsDestroyed());
    }
}
