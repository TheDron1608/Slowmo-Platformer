using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoofWiring : GenerateOnFinishLevelEnviroment
{
    const int HEIGHT = 3;

    public TileBase DrawTile;
    public int MaxLength = 35;

    public override void Generate()
    {
        bool leftOrRightDirection = NumberMath.RandomCoinflip();
        Vector3Int position = new Vector3Int((int)math.floor(transform.position.x), (int)math.floor(transform.position.y));
        MultiTileMapsContainer targetTilemaps = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;

        for (int y = position.y; y < HEIGHT + position.y; y++)
        {
            if (targetTilemaps.GetHasAnyTileAt(new Vector3Int(position.x, y))) return;
        }

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetHasAnyTileAt(new Vector3Int(x, position.y + HEIGHT)))
            {
                if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(new Vector3Int(x, position.y + HEIGHT)))
                {
                    ForceGenerate(targetTilemaps, position, leftOrRightDirection);
                }
                break;
            }
        }
    }

    private void ForceGenerate(MultiTileMapsContainer targetTilemaps, Vector3Int position, bool leftOrRightDirection)
    {
        Tilemap targetTilemap = targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS);

        for (int y = position.y; y < HEIGHT + position.y; y++)
        {
            targetTilemap.SetTile(new Vector3Int(position.x, y), DrawTile);
        }

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(new Vector3Int(x, position.y + HEIGHT)))
            {
                break;
            }
            else
            {
                targetTilemap.SetTile(new Vector3Int(x, position.y + HEIGHT), DrawTile);
            }
        }

        Destroy(gameObject);
    }
}
