using UnityEngine;

/// <summary>
/// will await ApplySender invoke to apply effects
/// </summary>
public abstract class AbstractEffectWithSender : AbstractEffect
{
    private MonoBehaviour _sender = null;

    public MonoBehaviour Sender
    {
        get => _sender;
        private set => _sender = value;
    }
    public void ApplySender(MonoBehaviour sender)
    {
        if (sender == null) throw new UnityException("sender argument can not be null");
        Sender = sender;
        OnReceivedSender(sender);
    }
    private void LateUpdate()
    {
        if (Sender == null)
        {
            throw new UnityException(gameObject.name + " not received sender at the end of the frame it was instanitiated");
        }
    }

    protected abstract void OnReceivedSender(MonoBehaviour sender);
}
