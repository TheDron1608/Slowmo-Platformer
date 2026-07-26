using UnityEngine;

public class DestroyBlocksOnRadius : AbstractEffect
{
    const int DESTROY_BACKGROUND_MARGIN = 2;

    public int Radius = 5;

    protected override void OnApply()
    {
        bool brokeAnything = false;
        int bgRadius = Radius - DESTROY_BACKGROUND_MARGIN;
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);
        Vector2Int centerPosition;
        if (AffectedObject.TryGetComponent(out Collider2D collider))
        {
            centerPosition = TileManager.PositionToTilePosition(GameObjectUtility.GetCenterOfCollider(collider));
        }
        else
        {
            centerPosition = TileManager.PositionToTilePosition(AffectedObject.transform.position);
        }

        for (int x = -Radius; x < Radius; x++)
        {
            for (int y = -Radius; y < Radius; y++)
            {
                if (x * x + y * y < Radius * Radius)
                {
                    Vector3Int tilePos = new(
                        centerPosition.x + x,
                        centerPosition.y + y,
                        0
                        );
                    brokeAnything |= layer.MultiTileMapsContainer.DestroyTileAt(tilePos, x * x + y * y < bgRadius * bgRadius, true);
                }
            }
        }

        if (brokeAnything)
        {
            GetComponentInChildren<AbstractSoundPlayer>().PlaySound(false, centerPosition);
        }

        base.OnApply();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Radius == (other as DestroyBlocksOnRadius).Radius;
    }
}