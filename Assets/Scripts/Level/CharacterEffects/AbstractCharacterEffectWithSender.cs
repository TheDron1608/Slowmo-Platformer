using UnityEngine;

/// <summary>
/// will await ApplySender invoke to apply effects
/// </summary>
public abstract class AbstractCharacterEffectWithSender : AbstractCharacterEffect
{
    private MonoBehaviour _sender;

    public MonoBehaviour Sender
    {
        get => _sender;
        private set => _sender = value;
    }
    public void ApplySender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (sender == null) throw new UnityException("sender argument can not be null");
        Sender = sender;
        OnReceivedSender(sender, receiverPart);
    }

    protected abstract void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart);
}
