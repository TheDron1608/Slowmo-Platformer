using System;
using System.Collections.Generic;
using UnityEngine;

public class DeflectRangedProjectileToOwner : AbstractRangedProjectileDeflection
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        Vector2 deflectorCenter;
        if (sender.TryGetComponent(out AbstractCharacterComponent charComponent))
        {
            deflectorCenter = charComponent.CharComponents.Center.transform.position;
        }
        else if (sender.TryGetComponent(out Collider2D collider))
        {
            deflectorCenter = VectorMath.Vec3ToVec2(sender.transform.position) + collider.offset;
        }
        else
        {
            deflectorCenter = sender.transform.position;
        }
        
        RangedProjectile.transform.position = RangedProjectile.ProjectileTip.position;
        RangedProjectile.MoveAlignVec2 = -VectorMath.Quartenion2DToVec2(Quaternion.FromToRotation(RangedProjectile.transform.position, RangedProjectile?.Owner.CharComponents.Center.transform.position ?? deflectorCenter));

        if (sender.TryGetComponent(out AbstractProjectile defclectorProjectile))
        {
            RangedProjectile.Owner = defclectorProjectile.Owner;
        }

        RemoveSelf();
    }
}
