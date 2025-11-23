using System;

public interface IEffectApplier
{
    public class OnEffectAppliedEventArgs
    {
        public IEffectApplier Sender;
        public AbstractEffect Effect;
        public ObjectEffectsReceiver Receiver;

        public OnEffectAppliedEventArgs(IEffectApplier sender, AbstractEffect effect, ObjectEffectsReceiver receiver)
        {
            Sender = sender;
            Effect = effect;
            Receiver = receiver;
        }
    }

    public event EventHandler<OnEffectAppliedEventArgs> OnEffectApplied;

    public void InvokeOnEffectApllied(AbstractEffect Effect, ObjectEffectsReceiver Receiver);
}
