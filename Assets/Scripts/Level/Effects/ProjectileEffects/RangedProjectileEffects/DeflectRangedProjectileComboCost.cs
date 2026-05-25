using Unity.Mathematics;
using UnityEngine;

public class DeflectRangedProjectileComboCost : AbstractRangedProjectileDeflection
{
    public float ComboMultOnDeflect = 0.5f;
    public float DeflectionAccuracy = 0.25f;

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

            ScoreManager.Instance.CurrentCombo = (int)math.floor(ScoreManager.Instance.CurrentCombo * ComboMultOnDeflect);
        }

        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && ScoreManager.Instance?.CurrentCombo > 0;
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            ComboMultOnDeflect == (other as DeflectRangedProjectileComboCost).ComboMultOnDeflect &&
            DeflectionAccuracy == (other as DeflectRangedProjectileComboCost).DeflectionAccuracy;
    }
}
