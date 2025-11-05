using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallWiring : GenerateOnFinishLevelEnviroment
{
    public TileBase DrawTile;
    public int MaxLength = 35;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        Vector3Int position = NumberMath.Vec3ToVec3Int(generationInfo.Offset);
        MultiTileMapsContainer targetTilemaps = generationInfo.GenerateWhere.MultiTileMapsContainer;

        if (targetTilemaps.GetForeground().GetTile(position) != null)
        {
            return null;
        }

        bool leftOrRightDirection = targetTilemaps.GetForeground().GetTile(position + Vector3Int.left) != null;

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetHasAnyTileAt(new Vector3Int(x, position.y)))
            {
                if (targetTilemaps.GetForeground().HasTile(new Vector3Int(x, position.y)))
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
        Tilemap targetTilemap = targetTilemaps.GetBackgroundDecorations();

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetForeground().HasTile(new Vector3Int(x, position.y)))
            {
                break;
            }
            else
            {
                targetTilemap.SetTile(new Vector3Int(x, position.y), DrawTile);
            }
        }

        return new List<GameObject> { targetTilemap.gameObject };
    }
}
