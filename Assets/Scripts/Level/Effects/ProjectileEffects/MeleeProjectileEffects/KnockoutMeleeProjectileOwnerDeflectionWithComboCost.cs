using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class KnockoutMeleeProjectileOwnerDeflectionWithComboCost : AbstractMeleeProjectileDeflection
{
    public float ComboMultOnDeflect = 0.5f;
    public List<AbstractEffect> ExtraKnockoutEffects = new();
    public float KnockoutVelocity = 40f;
    public Vector2 KnockoutDirection = Vector2.one;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        base.OnReceivedSender(sender);

        if (MeleeProjectile.Owner != null)
        {
            if (Sender.TryGetComponent(out AbstractCharacterComponent thrower))
            {
                thrower.CharComponents.CharacterVisual.DoACoolFlip();
            }

            MeleeProjectile.Owner.CharComponents.CharacterRigidBody.linearVelocity += new Vector2(
                Sender.transform.position.x < MeleeProjectile.Owner.transform.position.x ? KnockoutDirection.x : -KnockoutDirection.x,
                KnockoutDirection.y
                ).normalized * KnockoutVelocity;

            MeleeProjectile.Owner.CharComponents.CharacterEffectsReceiver.ApplyEffect(ExtraKnockoutEffects, Sender);

            ScoreManager.Instance.CurrentCombo = (int)math.floor(ScoreManager.Instance.CurrentCombo * ComboMultOnDeflect);
        }

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            ScoreManager.Instance?.CurrentCombo > 0 &&
            ((!affectWho.GetComponent<MeleeProjectile>().Owner?.gameObject.IsDestroyed()) ?? false);
    }

    public override List<AbstractEffect> GetSelfIncludeIncomingEffects()
    {
        return NumberMath.MergeLists(base.GetSelfIncludeIncomingEffects(), ExtraKnockoutEffects);
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            ComboMultOnDeflect == (other as KnockoutMeleeProjectileOwnerDeflectionWithComboCost).ComboMultOnDeflect &&
            KnockoutVelocity == (other as KnockoutMeleeProjectileOwnerDeflectionWithComboCost).KnockoutVelocity &&
            ExtraKnockoutEffects == (other as KnockoutMeleeProjectileOwnerDeflectionWithComboCost).ExtraKnockoutEffects &&
            KnockoutDirection == (other as KnockoutMeleeProjectileOwnerDeflectionWithComboCost).KnockoutDirection;
    }
}
