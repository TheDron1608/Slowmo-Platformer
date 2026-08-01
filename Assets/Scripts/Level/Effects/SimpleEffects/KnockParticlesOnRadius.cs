using UnityEngine;

public class KnockParticlesOnRadius : AbstractEffect
{
    const int DESTROY_BACKGROUND_MARGIN = 2;

    public int Radius = 5;
    public float Knockback = 10f;

    protected override void OnApply()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);
        Vector3 centerPosition;
        if (AffectedObject.TryGetComponent(out Collider2D collider))
        {
            centerPosition = GameObjectUtility.GetCenterOfCollider(collider);
        }
        else
        {
            centerPosition = AffectedObject.transform.position;
        }

        foreach (Transform physicsParticle in layer.PhysicsParticlesContainer)
        {
            float distance = Vector2.Distance(physicsParticle.position, centerPosition);
            if (
                distance < Radius &&
                physicsParticle.TryGetComponent(out Rigidbody2D rb)
                )
            {
                rb.simulated = true;
                rb.linearVelocity += VectorMath.Vec3ToVec2(physicsParticle.position - centerPosition).normalized * (distance / Radius) * Knockback;
            }
        }

        base.OnApply();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Radius == (other as KnockParticlesOnRadius).Radius &&
            Knockback == (other as KnockParticlesOnRadius).Knockback;
    }
}