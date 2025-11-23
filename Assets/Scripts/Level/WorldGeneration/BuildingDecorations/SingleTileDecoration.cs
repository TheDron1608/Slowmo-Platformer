using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SingleTileDecoration : GenerateOnFinishLevelEnviroment
{
    public TileBase DrawTile;
    public TileBase MirroredDrawTile;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer targetTilemap = generationInfo.GenerateWhere.MultiTileMapsContainer;
        Vector3Int targetPosition = NumberMath.Vec3ToVec3Int(generationInfo.Offset);

        if (targetTilemap.GetHasAnyTileAt(targetPosition))
        {
            return null;
        }

        if (targetTilemap.GetForeground().HasTile(targetPosition + Vector3Int.left))
        {
            targetTilemap.GetBackgroundDecorations().SetTile(targetPosition, DrawTile);
        }
        else if (targetTilemap.GetForeground().HasTile(targetPosition + Vector3Int.right))
        {
            targetTilemap.GetBackgroundDecorations().SetTile(targetPosition, MirroredDrawTile);
        }
        else
        {
            targetTilemap.GetBackgroundDecorations().SetTile(
                targetPosition,
                NumberMath.RandomCoinflip() ? MirroredDrawTile : DrawTile
                );
        }

        return new List<GameObject> { targetTilemap.GetBackgroundDecorations().gameObject };
    }
}
