using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingLandBottom : GenerateOnFinishAllBuildingEnviroment
{
    const int MIN_VERTICAL_RANGE = 20;

    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBase OvergoundFillTile;
    public int ExtraWidth = 0;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer generateWhere = generationInfo.GenerateWhere.MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetForeground();
        Tilemap targetOvergoundTilemap = generateWhere.GetOverground();

        int x1 = (int)math.floor(generationInfo.Offset.x + OffsetStart.transform.position.x) - ExtraWidth;
        int y1 = BuildingInfo.GlobalLowestCoorY - MIN_VERTICAL_RANGE;
        int x2 = (int)math.floor(generationInfo.Offset.x + OffsetEnd.transform.position.x) + ExtraWidth;
        int y2 = (int)math.floor(generationInfo.Offset.y + math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y2; y >= y1; y--)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (
                    !generateWhere.GetForeground().HasTile(tilePos) &&
                    !generateWhere.GetBackground().HasTile(tilePos)
                    )
                {
                    targetTilemap.SetTile(tilePos, FillTile);
                    targetOvergoundTilemap.SetTile(tilePos, OvergoundFillTile);
                }
            }
        }

        return new List<GameObject> { targetTilemap.gameObject };
    }

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        if (building != null)
        {
            int posY = (int)(position.y + math.min(OffsetEnd.transform.position.y, OffsetStart.transform.position.x));
            if (building.LowestCoorY > posY) building.LowestCoorY = posY;
            if (BuildingInfo.GlobalLowestCoorY > posY) BuildingInfo.GlobalLowestCoorY = posY;
        }
        return base.PreGenerate(preGenerateWhere, position, building, chunk);
    }
}
