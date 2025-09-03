using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Vent : GenerateOnFinishLevelEnviroment
{
    public int MinWidth = 1;
    public int MaxWidth = 8;
    public TileBase DrawTile;

    public override void Generate()
    {
        int targetHeight = NumberMath.PickRandomInRangeNoSeed(MinWidth, MaxWidth);
        MultiTileMapsContainer targetTilemap = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;
        Vector3Int targetPosition = new((int)math.floor(transform.position.x), (int)math.floor(transform.position.y));
        Vector3Int targetGenerateDirection = NumberMath.RandomCoinflip() ? Vector3Int.left : Vector3Int.right;

        for (int i = 0; i < targetHeight; i++)
        {
            if (targetTilemap.GetHasAnyTileAt(targetPosition + (Vector3Int.up * i))) return;
        }

        for (int i = 0; i < targetHeight; i++)
        {
            if (
                targetTilemap.GetHasAnyTileAt(targetPosition + (targetGenerateDirection * i)) ||
                !targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(targetPosition + (targetGenerateDirection * i) + Vector3Int.down)
                )
            {
                return;
            }
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(targetPosition + (targetGenerateDirection * i), DrawTile);
        }
    }
}
