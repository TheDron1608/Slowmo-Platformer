using System.Collections.Generic;
using System.Linq;
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
    public TileBase OvergoundRoofFillTile;
    public int RoofHeight = 3;

    public override void Generate()
    {
        MultiTileMapsContainer generateWhere = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;

        int x1 = (int)math.floor(OffsetStart.transform.position.x);
        int y1 = (int)math.floor(math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));
        int x2 = (int)math.floor(OffsetEnd.transform.position.x);
        int y2 = (int)math.floor(LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).WorldGenerationDataObjectsContainer.GetComponentsInChildren<BuildingRoofedTop>().OrderBy(r => r.OffsetStart.transform.position.y).Last().OffsetStart.transform.position.y);

        Tilemap targetTilemap = generateWhere.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.NORMAL);
        Tilemap targetOvergoundTilemap = generateWhere.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.OVERGROUND);

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

        if (RoofFillTile != null)
        {
            for (int x = x1; x <= x2; x++)
            {
                for (int y = y2; y <= y2 + RoofHeight; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y);

                    if (!generateWhere.GetHasAnyTileAt(tilePos))
                    {
                        targetTilemap.SetTile(tilePos, RoofFillTile);
                        targetOvergoundTilemap.SetTile(tilePos, OvergoundRoofFillTile);
                    }
                }
            }
        }

        if (UnityEngine.Random.value < BuildingRoofDecorationsGenerationChance)
        {
            GameObject newDecoration = Instantiate(
                NumberMath.PickRandomItem(AvaibleBuildingRoofDecorations).gameObject,
                new Vector3(NumberMath.PickRandomInRangeNoSeed(x1, x2), y2 + RoofHeight + 1),
                transform.rotation,
                LayerManager.Instance.GetZLayerOfGameObject(gameObject).WorldGenerationDataObjectsContainer
                );
            LayerManager.Instance.GetZLayerOfGameObject(newDecoration).UpdateLayerForGameObject(newDecoration);
        }

        for (int i = 0; i < math.abs((y1 - y2) / BuildingWallDecorationsPerHeight); i++)
        {
            GameObject newDecoration = Instantiate(
                NumberMath.PickRandomItem(AvaibleBuildingWallDecorations).gameObject,
                new Vector3(NumberMath.RandomCoinflip() ? x1 - 1 : x2 + 1, NumberMath.PickRandomInRangeNoSeed(y1, y2)),
                transform.rotation,
                LayerManager.Instance.GetZLayerOfGameObject(gameObject).WorldGenerationDataObjectsContainer
                );
            LayerManager.Instance.GetZLayerOfGameObject(newDecoration).UpdateLayerForGameObject(newDecoration);
            LayerManager.Instance.GetZLayerOfGameObject(newDecoration).TileManager.Debug_MarkTile(new Vector2(NumberMath.RandomCoinflip() ? x1 - 1 : x2 + 1, NumberMath.PickRandomInRangeNoSeed(y1, y2)), Color.red, 999f);
        }

        Destroy(gameObject);
    }
}
