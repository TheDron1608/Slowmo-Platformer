using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Anthen : GenerateOnFinishLevelEnviroment
{
    const int MIN_HEIGHT = 1;
    const int MAX_HEIGHT = 3;

    public TileBase DrawTile;

    public override void Generate()
    {
        int targetHeight = NumberMath.PickRandomInRangeNoSeed(MIN_HEIGHT, MAX_HEIGHT);
        MultiTileMapsContainer targetTilemap = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;
        Vector3Int targetPosition = new((int)math.floor(transform.position.x), (int)math.floor(transform.position.y));

        for (int i = 0; i < targetHeight; i++)
        {
            if (targetTilemap.GetHasAnyTileAt(targetPosition + (Vector3Int.up * i))) return;
        }

        for (int i = 0; i < targetHeight; i++)
        {
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(targetPosition + (Vector3Int.up * i), DrawTile);
        }
    }
}
