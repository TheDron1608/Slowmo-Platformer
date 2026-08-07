using System;
using System.Collections.Generic;

public interface IEffectApplier
{
    public class OnEffectAppliedEventArgs
    {
        public IEffectApplier Sender;
        public AbstractEffect Effect;
        public ObjectEffectsReceiver Receiver;
        public List<IEffectApplier> Appliers;

        public OnEffectAppliedEventArgs(IEffectApplier sender, AbstractEffect effect, ObjectEffectsReceiver receiver, List<IEffectApplier> appliers)
        {
            Sender = sender;
            Effect = effect;
            Receiver = receiver;
            Appliers = appliers;
        }
    }

    public event EventHandler<OnEffectAppliedEventArgs> OnEffectApplied;

    public void InvokeOnEffectApllied(AbstractEffect effect, ObjectEffectsReceiver receiver, List<IEffectApplier> appliers);
}
