using UnityEngine;

public class SleepingCarpProjectileDeflection : AbstractRangedProjectileDeflection
{
    public float DeflectionAccuracy = 0.25f;
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (Sender.TryGetComponent(out AbstractCharacterComponent characterSender) && characterSender.CharComponents.CharacterHolding.CurrentHoldObject == null)
        {
            base.OnReceivedSender(sender);

            characterSender.CharComponents.CharacterVisual.DoACoolFlip();

            Vector2 newAlign = Vector2.Reflect(
                RangedProjectile.MoveAlignVec2, 
                VectorMath.Quartenion2DToVec2(VectorMath.RandomizeQuarternion(Quaternion.FromToRotation(characterSender.CharComponents.Center.transform.position, RangedProjectile.transform.position), DeflectionAccuracy))
                );

            RangedProjectile.MoveAlignVec2 = newAlign;
        }

        RemoveSelf();
    }
}
