using UnityEngine;

public class DeflectRangedProjectileComboCost : AbstractRangedProjectileDeflection
{
    public float DeflectionAccuracy = 0.25f;
    public int DeflectComboCost = 1;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (Sender.TryGetComponent(out AbstractCharacterComponent characterSender))
        {
            base.OnReceivedSender(sender);

            characterSender.CharComponents.CharacterVisual.DoACoolFlip();

            Vector2 newAlign = Vector2.Reflect(
                RangedProjectile.MoveAlignVec2,
                VectorMath.Quartenion2DToVec2(VectorMath.RandomizeQuarternion(Quaternion.FromToRotation(characterSender.CharComponents.Center.transform.position, RangedProjectile.transform.position), DeflectionAccuracy))
                );

            RangedProjectile.MoveAlignVec2 = newAlign;

            ScoreManager.Instance.CurrentCombo -= DeflectComboCost;
        }

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && ScoreManager.Instance?.CurrentCombo >= DeflectComboCost;
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            DeflectComboCost == (other as DeflectRangedProjectileComboCost).DeflectComboCost &&
            DeflectionAccuracy == (other as DeflectRangedProjectileComboCost).DeflectionAccuracy;
    }
}
