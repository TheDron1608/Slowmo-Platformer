using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Anthen : GenerateOnFinishLevelEnviroment
{
    const int MIN_HEIGHT = 1;
    const int MAX_HEIGHT = 3;

    public TileBase DrawTile;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        int targetHeight = NumberMath.PickRandomInRangeNoSeed(MIN_HEIGHT, MAX_HEIGHT);
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
            targetTilemap.GetBackgroundDecorations().SetTile(targetPosition + (Vector3Int.up * i), DrawTile);
        }

        return new List<GameObject> { targetTilemap.GetBackgroundDecorations().gameObject };
    }
}
