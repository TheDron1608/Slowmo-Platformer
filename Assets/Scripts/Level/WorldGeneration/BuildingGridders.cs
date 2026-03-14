using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingGridders : GenerateBeforeExtraChunksEnviroment
{
    const int MIN_VERTICAL_RANGE = 20;

    public TileBase Gridders;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer generateWhere = generationInfo.GenerateWhere.MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetBackgroundDecorations();

        int x = (int)math.floor(generationInfo.Offset.x + transform.position.x);
        int y1 = BuildingInfo.GlobalLowestCoorY - MIN_VERTICAL_RANGE;
        int y2 = (int)math.floor(generationInfo.Offset.y + transform.position.y);

        for (int y = y2; y >= y1; y--)
        {
            Vector3Int tilePos = new Vector3Int(x, y);

            if (
                generateWhere.GetForeground().HasTile(tilePos) ||
                (
                    generateWhere.GetBackground().HasTile(tilePos) && 
                    ((!generateWhere.GetBackground().GetTile<BackgroundRuleTile>(tilePos)?.CanBeOverridedByGridders) ?? 
                        generateWhere.GetBackground().GetTile<RestrictInteriourWalls>(tilePos) == null))
                )
            {
                break;
            }

            targetTilemap.SetTile(tilePos, Gridders);
        }

        return new List<GameObject> { targetTilemap.gameObject };
    }

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        if (building != null)
        {
            int posY = (int)(position.y + transform.position.y);
            if (building.LowestCoorY > posY) building.LowestCoorY = posY;
            if (BuildingInfo.GlobalLowestCoorY > posY) BuildingInfo.GlobalLowestCoorY = posY;
        }
        return base.PreGenerate(preGenerateWhere, position, building, chunk);
    }
}
