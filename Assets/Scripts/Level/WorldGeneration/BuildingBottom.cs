using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingBottom : GenerateBeforeExtraChunksEnviroment
{
    const int MIN_VERTICAL_RANGE = 20;
    const int VERTICAL_GRIDDER_RATE = 10;
    const int HORIZONTAL_GRIDDER_RATE = 15;
    const float VERTICAL_GRIDDER_SPAWN_CHANCE = 0.825f;
    const float HORIZONTAL_GRIDDER_SPAWN_CHANCE = 0.75f;

    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBase OvergoundFillTile;
    public TileBase ColumnFillTile;
    public int BuildingWallDecorationsPerHeight = 35;
    public List<GenerateOnFinishLevelEnviroment> AvaibleBuildingWallDecorations = new();

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        MultiTileMapsContainer generateWhere = generationInfo.GenerateWhere.MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetForeground();
        Tilemap targetOvergoundTilemap = generateWhere.GetOverground();

        int x1 = (int)math.floor(generationInfo.Offset.x + OffsetStart.transform.position.x);
        int y1 = BuildingInfo.GlobalLowestCoorY - MIN_VERTICAL_RANGE;
        int x2 = (int)math.floor(generationInfo.Offset.x + OffsetEnd.transform.position.x);
        int y2 = (int)math.floor(generationInfo.Offset.y + math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));
        bool allColumsValid = true;

        for (int x = x1; x <= x2; x++)
        {
            bool isValidColumn = true;
            for (int y = y2; y >= y1; y--)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (
                    generateWhere.GetForeground().HasTile(tilePos) ||
                    generateWhere.GetBackground().HasTile(tilePos)
                    )
                {
                    isValidColumn = false;
                    allColumsValid = false;
                    break;
                }
            }

            if (isValidColumn)
            {
                for (int y = y2; y >= y1; y--)
                {
                    Vector3Int tilePos = new Vector3Int(x, y);

                    targetTilemap.SetTile(tilePos, FillTile);
                    targetOvergoundTilemap.SetTile(tilePos, OvergoundFillTile);
                }
            }
            else if (x % VERTICAL_GRIDDER_RATE == 0 && UnityEngine.Random.value < VERTICAL_GRIDDER_SPAWN_CHANCE)
            {
                Tilemap targetColumnTilemap = generateWhere.GetBackgroundDecorations();
                bool validForColumn = true;
                for (int y = y2; y >= y1; y--)
                {
                    Vector3Int tilePos = new Vector3Int(x, y);

                    if (targetColumnTilemap.HasTile(tilePos))
                    {
                        validForColumn = false;
                        break;
                    }
                }

                if (validForColumn)
                {
                    for (int y = y2; y >= y1; y--)
                    {
                        Vector3Int tilePos = new Vector3Int(x, y);

                        if (
                            (
                                !generateWhere.GetForeground().HasTile(tilePos) || 
                                generateWhere.GetForeground().GetTile(tilePos) is RestrictInteriourWalls
                            ) &&
                            (
                                !generateWhere.GetBackground().HasTile(tilePos) || 
                                generateWhere.GetBackground().GetTile(tilePos) is RestrictInteriourWalls ||
                                generateWhere.GetBackground().GetTile<BackgroundRuleTile>(tilePos).CanBeOverridedByGridders
                            )
                            )
                        {
                            targetColumnTilemap.SetTile(tilePos, ColumnFillTile);

                            if (y % HORIZONTAL_GRIDDER_RATE == 0 && UnityEngine.Random.value < HORIZONTAL_GRIDDER_SPAWN_CHANCE)
                            {
                                if (targetColumnTilemap.HasTile(tilePos + Vector3Int.left * VERTICAL_GRIDDER_RATE))
                                {
                                    for (int subX = x; subX >= x - VERTICAL_GRIDDER_RATE; subX--)
                                    {
                                        Vector3Int subTilePos = new Vector3Int(subX, y);
                                        targetColumnTilemap.SetTile(subTilePos, ColumnFillTile);
                                    }
                                }
                                else if (targetColumnTilemap.HasTile(tilePos + Vector3Int.right * VERTICAL_GRIDDER_RATE))
                                {
                                    for (int subX = x; subX <= x + VERTICAL_GRIDDER_RATE; subX++)
                                    {
                                        Vector3Int subTilePos = new Vector3Int(subX, y);
                                        targetColumnTilemap.SetTile(subTilePos, ColumnFillTile);
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        if (allColumsValid)
        {
            for (int i = 0; i < math.abs((y1 - y2) / BuildingWallDecorationsPerHeight); i++)
            {
                NumberMath.PickRandomItem(AvaibleBuildingWallDecorations).PreGenerate(
                    generationInfo.GenerateWhere,
                    new Vector3(NumberMath.RandomCoinflip() ? x1 - 1 : x2 + 1, NumberMath.PickRandomInRangeNoSeed(y1, y2)),
                    generationInfo.Building,
                    generationInfo.Chunk
                    );
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
