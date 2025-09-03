using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SingleTileDecoration : GenerateOnFinishLevelEnviroment
{
    public TileBase DrawTile;
    public TileBase MirroredDrawTile;

    public override void Generate()
    {
        MultiTileMapsContainer targetTilemap = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;
        Vector3Int targetPosition = new((int)math.floor(transform.position.x), (int)math.floor(transform.position.y));

        if (targetTilemap.GetHasAnyTileAt(targetPosition)) return;

        if (targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(targetPosition + Vector3Int.left))
        {
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(targetPosition, DrawTile);
        }
        else if (targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(targetPosition + Vector3Int.right))
        {
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(targetPosition, MirroredDrawTile);
        }
        else
        {
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(
                targetPosition, 
                NumberMath.RandomCoinflip() ? MirroredDrawTile : DrawTile
                );
        }
    }
}
