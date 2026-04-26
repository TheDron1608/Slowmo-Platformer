using System;
using Unity.VisualScripting;
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
        if (sender != null || GetType().HasAttribute<AllowEffectWithSenderReceiveNull>())
        {
            Sender = sender;
        }
        else
        {
            throw new UnityException("sender argument can not be null, use AllowEffectWithSenderReceiveNull to allow use null as receiver");
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (Sender == null)
        {
            if (GetType().HasAttribute<AllowEffectWithSenderReceiveNull>())
            {
                OnReceivedSender(null);
            }
            else
            {
                throw new UnityException(gameObject.name + " not received sender at the end of the frame it was instanitiated, use AllowEffectWithSenderReceiveNull to allow use null as receiver");
            }
        }
        else
        {
            OnReceivedSender(Sender);
        }
    }

    protected abstract void OnReceivedSender(MonoBehaviour sender);
}

public class AllowEffectWithSenderReceiveNull : Attribute
{

}