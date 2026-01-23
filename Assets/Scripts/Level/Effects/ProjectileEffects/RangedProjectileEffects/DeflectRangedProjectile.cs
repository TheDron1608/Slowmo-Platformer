using UnityEngine;

public class DeflectRangedProjectile : AbstractRangedProjectileDeflection
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        base.OnReceivedSender(sender);

        Vector2 newAlign = Vector2.Reflect(RangedProjectile.MoveAlignVec2, VectorMath.Quartenion2DToVec2(Quaternion.FromToRotation(sender.transform.position, RangedProjectile.transform.position)));

        RangedProjectile.transform.position = RangedProjectile.ProjectileTip.position;
        RangedProjectile.MoveAlignVec2 = newAlign;

        if (sender.TryGetComponent(out AbstractProjectile defclectorProjectile))
        {
            RangedProjectile.Owner = defclectorProjectile.Owner;
        }

        RemoveSelf();
    }
}
