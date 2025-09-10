using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Vent : GenerateOnFinishLevelEnviroment
{
    public int MinWidth = 1;
    public int MaxWidth = 8;
    public TileBase DrawTile;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        int targetHeight = NumberMath.PickRandomInRangeNoSeed(MinWidth, MaxWidth);
        Vector3Int targetGenerateDirection = NumberMath.RandomCoinflip() ? Vector3Int.left : Vector3Int.right;

        MultiTileMapsContainer targetTilemap = generationInfo.GenerateWhere.MultiTileMapsContainer;
        Vector3Int targetPosition = NumberMath.Vec3ToVec3Int(generationInfo.Offset);

        for (int i = 0; i < targetHeight; i++)
        {
            if (targetTilemap.GetHasAnyTileAt(targetPosition + (Vector3Int.up * i)))
            {
                return null;
            }
        }

        for (int i = 0; i < targetHeight; i++)
        {
            if (
                targetTilemap.GetHasAnyTileAt(targetPosition + (targetGenerateDirection * i)) ||
                !targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL).HasTile(targetPosition + (targetGenerateDirection * i) + Vector3Int.down)
                )
            {
                break;
            }
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(targetPosition + (targetGenerateDirection * i), DrawTile);
        }

        return new List<GameObject> { targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).gameObject };
    }
}
