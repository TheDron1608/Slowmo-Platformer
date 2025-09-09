using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallWiring : GenerateOnFinishLevelEnviroment
{
    public TileBase DrawTile;
    public int MaxLength = 35;

    public override List<GameObject> Generate()
    {
        Vector3Int position = new Vector3Int((int)math.floor(transform.position.x), (int)math.floor(transform.position.y));
        MultiTileMapsContainer targetTilemaps = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;

        if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).GetTile(position) != null) return null;

        bool leftOrRightDirection = targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).GetTile(position + Vector3Int.left) != null;

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetHasAnyTileAt(new Vector3Int(x, position.y)))
            {
                if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(new Vector3Int(x, position.y)))
                {
                    return ForceGenerate(targetTilemaps, position, leftOrRightDirection);
                }
                break;
            }
        }

        return null;
    }

    private List<GameObject> ForceGenerate(MultiTileMapsContainer targetTilemaps, Vector3Int position, bool leftOrRightDirection)
    {
        Tilemap targetTilemap = targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS);

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(new Vector3Int(x, position.y)))
            {
                break;
            }
            else
            {
                targetTilemap.SetTile(new Vector3Int(x, position.y), DrawTile);
            }
        }

        Destroy(gameObject);

        return new List<GameObject> { targetTilemap.gameObject };
    }
}
