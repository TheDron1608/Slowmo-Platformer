using UnityEngine;

public class SpawnProjectilesFromCenter : AbstractEffect
{
    public AbstractProjectile Projectile;
    public int ProjectilesAmount = 1;

    protected override void OnApply()
    {
        base.OnApply();

        Vector2 centerPosition;
        if (AffectedObject.TryGetComponent(out Collider2D collider))
        {
            centerPosition = GameObjectUtility.GetCenterOfCollider(collider);
        }
        else
        {
            centerPosition = AffectedObject.transform.position;
        }

        for (int i = 0; i < ProjectilesAmount; i++)
        {
            Projectile.SpawnProjectile(
                Vector2.up,
                centerPosition,
                LayerManager.Instance.GetZLayerOfGameObject(gameObject),
                AffectedObject.TryGetComponent(out IEffectApplier effectApplier) ? effectApplier as MonoBehaviour : AffectedObject,
                0f
                );
        }

        RemoveSelf();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Projectile == (other as SpawnProjectilesFromCenter).Projectile &&
            ProjectilesAmount == (other as SpawnProjectilesFromCenter).ProjectilesAmount;
    }
}