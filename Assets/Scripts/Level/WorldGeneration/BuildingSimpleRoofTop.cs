using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSimpleRoofTop : GenerateBeforeExtraChunksEnviroment
{
    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase RoofFillTile;
    public TileBase RoofOvergroundFillTile;
    public int RoofHeight = 3;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer generateWhere = generationInfo.GenerateWhere.MultiTileMapsContainer;

        int x1 = (int)math.floor(generationInfo.Offset.x + OffsetStart.transform.position.x);
        int y1 = (int)math.floor(generationInfo.Offset.y + math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));
        int x2 = (int)math.floor(generationInfo.Offset.x + OffsetEnd.transform.position.x);
        int y2 = y1 + RoofHeight;

        Tilemap targetTilemap = generateWhere.GetForeground();
        Tilemap targetOvergoundTilemap = generateWhere.GetOverground();

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (
                    generateWhere.GetHasAnyTileAt(tilePos) &&
                    (targetOvergoundTilemap.GetTile(tilePos) != RoofOvergroundFillTile)
                    )
                {
                    return new List<GameObject> { targetTilemap.gameObject };
                }
            }
        }

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (
                    !generateWhere.GetHasAnyTileAt(tilePos) || 
                    (targetOvergoundTilemap.GetTile(tilePos) == RoofOvergroundFillTile)
                    )
                {
                    targetTilemap.SetTile(tilePos, RoofFillTile);
                    targetOvergoundTilemap.SetTile(tilePos, RoofOvergroundFillTile);
                }
                else
                {
                    break;
                }
            }
        }

        return new List<GameObject> { targetTilemap.gameObject };
    }

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        if (building != null)
        {
            int posY = (int)(position.y + math.max(OffsetEnd.transform.position.y, OffsetStart.transform.position.x));
            if (building.HighestCoorY < posY) building.HighestCoorY = posY;
            if (BuildingInfo.GlobalHighestCoorY < posY) BuildingInfo.GlobalHighestCoorY = posY;
        }
        return base.PreGenerate(preGenerateWhere, position, building, chunk);
    }
}
