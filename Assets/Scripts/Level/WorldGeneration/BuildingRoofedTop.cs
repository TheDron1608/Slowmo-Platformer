using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingRoofedTop : GenerateOnFinishBuildingEnviroment
{
    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBase OvergoundFillTile;
    public float BuildingRoofDecorationsGenerationChance = 0.25f;
    public List<GenerateOnFinishLevelEnviroment> AvaibleBuildingRoofDecorations = new();
    public int BuildingWallDecorationsPerHeight = 25;
    public List<GenerateOnFinishLevelEnviroment> AvaibleBuildingWallDecorations = new();
    public TileBase RoofFillTile;
    public TileBase RoofOvergroundFillTile;
    public int RoofHeight = 3;
    public int RoofSeparationMinHeightDifference = 25;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer generateWhere = generationInfo.GenerateWhere.MultiTileMapsContainer;

        int x1 = (int)math.floor(generationInfo.Offset.x + OffsetStart.transform.position.x);
        int y1 = (int)math.floor(generationInfo.Offset.y + math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));
        int x2 = (int)math.floor(generationInfo.Offset.x + OffsetEnd.transform.position.x);
        int y2 = generationInfo.Building.HighestCoorY;

        Tilemap targetTilemap = generateWhere.GetForeground();
        Tilemap targetOvergoundTilemap = generateWhere.GetOverground();

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (!generateWhere.GetHasAnyTileAt(tilePos))
                {
                    targetTilemap.SetTile(tilePos, FillTile);
                    targetOvergoundTilemap.SetTile(tilePos, OvergoundFillTile);
                }
            }
        }

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y2; y <= y2 + RoofHeight; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (!generateWhere.GetHasAnyTileAt(tilePos))
                {
                    targetTilemap.SetTile(tilePos, RoofFillTile);
                    targetOvergoundTilemap.SetTile(tilePos, RoofOvergroundFillTile);
                }
            }
        }

        if (UnityEngine.Random.value < BuildingRoofDecorationsGenerationChance)
        {
            NumberMath.PickRandomItem(AvaibleBuildingRoofDecorations).PreGenerate(
                generationInfo.GenerateWhere,
                new Vector3(NumberMath.PickRandomInRangeNoSeed(x1, x2), y2 + RoofHeight + 1),
                generationInfo.Building,
                generationInfo.Chunk
                );
        }

        for (int i = 0; i < math.abs((y1 - y2) / BuildingWallDecorationsPerHeight); i++)
        {
            NumberMath.PickRandomItem(AvaibleBuildingWallDecorations).PreGenerate(
                generationInfo.GenerateWhere,
                new Vector3(NumberMath.RandomCoinflip() ? x1 - 1 : x2 + 1, NumberMath.PickRandomInRangeNoSeed(y1, y2)),
                generationInfo.Building,
                generationInfo.Chunk
                );
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
