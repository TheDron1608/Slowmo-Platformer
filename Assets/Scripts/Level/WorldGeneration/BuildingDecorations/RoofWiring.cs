using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoofWiring : GenerateOnFinishLevelEnviroment
{
    const int HEIGHT = 3;

    public TileBase DrawTile;
    public int MaxLength = 35;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        bool leftOrRightDirection = NumberMath.RandomCoinflip();
        Vector3Int position = NumberMath.Vec3ToVec3Int(generationInfo.Offset);
        MultiTileMapsContainer targetTilemaps =generationInfo.GenerateWhere.MultiTileMapsContainer;

        for (int y = position.y; y < HEIGHT + position.y; y++)
        {
            if (targetTilemaps.GetHasAnyTileAt(new Vector3Int(position.x, y)))
            {
                return null;
            }
        }

        for (
            int x = position.x;
            leftOrRightDirection ? (x < position.x + MaxLength) : (x > position.x - MaxLength);
            x += leftOrRightDirection ? 1 : -1
            )
        {
            if (targetTilemaps.GetHasAnyTileAt(new Vector3Int(x, position.y + HEIGHT)))
            {
                if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).HasTile(new Vector3Int(x, position.y + HEIGHT)))
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
            if (targetTilemaps.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).HasTile(new Vector3Int(x, position.y + HEIGHT)))
            {
                break;
            }
            else
            {
                targetTilemap.SetTile(new Vector3Int(x, position.y + HEIGHT), DrawTile);
            }
        }

        return new List<GameObject> { targetTilemap.gameObject };
    }
}
