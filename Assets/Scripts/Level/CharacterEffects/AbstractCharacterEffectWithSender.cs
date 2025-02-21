using UnityEngine;

/// <summary>
/// will await ApplySender invoke to apply effects
/// </summary>
public abstract class AbstractCharacterEffectWithSender : AbstractCharacterEffect
{
    public void ApplySender(MonoBehaviour sender)
    {
        if (sender == null) throw new UnityException("sender argument can not be null");
        OnReceivedSender(sender);
    }

    protected abstract void OnReceivedSender(MonoBehaviour sender);
}
