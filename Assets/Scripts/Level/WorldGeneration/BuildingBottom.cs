using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingBottom : GenerateOnFinishLevelEnviroment
{
    const int MIN_VERTICAL_RANGE = 50;

    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBehaviour.TileBehaviourType FillTileType;

    public override void Generate()
    {
        float lowestBuildingBottomPosition = OffsetStart.transform.position.y;
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (BuildingBottom bottom in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<BuildingBottom>(true))
            {
                if (bottom.OffsetStart.transform.position.y < lowestBuildingBottomPosition)
                {
                    lowestBuildingBottomPosition = bottom.OffsetStart.transform.position.y;
                }
            }
        }

        MultiTileMapsContainer generateWhere = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetTileMapByBehaviourType(FillTileType);

        int x1 = (int)math.floor(OffsetStart.transform.position.x);
        int y1 = (int)math.floor(lowestBuildingBottomPosition) - MIN_VERTICAL_RANGE;
        int x2 = (int)math.floor(OffsetEnd.transform.position.x);
        int y2 = (int)math.floor(math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));

        for (int x = x1; x <= x2; x++)
        {
            for (int y = y1; y <= y2; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (!generateWhere.GetHasAnyTileAt(tilePos))
                {
                    targetTilemap.SetTile(tilePos, FillTile);
                }
            }
        }
        
        Destroy(gameObject);
    }
}
