using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingBottom : GenerateOnFinishAllBuildingEnviroment
{
    const int MIN_VERTICAL_RANGE = 50;

    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBase OvergoundFillTile;
    public int BuildingWallDecorationsPerHeight = 35;
    public List<GenerateOnFinishLevelEnviroment> AvaibleBuildingWallDecorations = new();

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer generateWhere = generationInfo.GenerateWhere.MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND);
        Tilemap targetOvergoundTilemap = generateWhere.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.OVERGROUND);

        int x1 = (int)math.floor(generationInfo.Offset.x + OffsetStart.transform.position.x);
        int y1 = BuildingInfo.GlobalLowestCoorY - MIN_VERTICAL_RANGE;
        int x2 = (int)math.floor(generationInfo.Offset.x + OffsetEnd.transform.position.x);
        int y2 = (int)math.floor(generationInfo.Offset.y + math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));

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
            int posY = (int)(position.y + math.min(OffsetEnd.transform.position.y, OffsetStart.transform.position.x));
            if (building.LowestCoorY > posY) building.LowestCoorY = posY;
            if (BuildingInfo.GlobalLowestCoorY > posY) BuildingInfo.GlobalLowestCoorY = posY;
        }
        return base.PreGenerate(preGenerateWhere, position, building, chunk);
    }
}
