
using Unity.VisualScripting;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class RestrictThrowHoldable : AbstractCharacterEffectWithSender, IEntireCharacterEffect
{
    private Holdable _permanentHoldable = null;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (sender?.TryGetComponent(out Holdable holdableSender) ?? false)
        {
            _permanentHoldable = holdableSender;
        }
        AffectedCharacter.CharacterHolding.IsAbleToThrowObjects = false;
    }

    private void FixedUpdate()
    {
        if (_permanentHoldable != null && !_permanentHoldable.IsDestroyed() && AffectedCharacter.CharacterHolding.CurrentHoldObject != _permanentHoldable)
        {
            AffectedCharacter.CharacterHolding.ForceGrab(_permanentHoldable);
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterHolding.IsAbleToThrowObjects = true;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<RestrictThrowHoldable>();
    }
}