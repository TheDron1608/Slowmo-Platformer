using System.Collections.Generic;
using Unity.Mathematics;
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

        if (targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).HasTile(targetPosition + Vector3Int.left))
        {
            targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).SetTile(targetPosition, DrawTile);
        }
        else if (targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).HasTile(targetPosition + Vector3Int.right))
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

        return new List<GameObject> { targetTilemap.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS).gameObject };
    }
}
