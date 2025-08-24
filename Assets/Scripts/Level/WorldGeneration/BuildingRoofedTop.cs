using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingRoofedTop : GenerateOnFinishBuildingEnviroment
{
    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBase RoofTile;
    public int RoofHeight = 3;
    public TileBehaviour.TileBehaviourType FillTileType;

    public override void Generate()
    {
        MultiTileMapsContainer generateWhere = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetTileMapByBehaviourType(FillTileType);

        int x1 = (int)math.floor(OffsetStart.transform.position.x);
        int y1 = (int)math.floor(math.max(OffsetStart.transform.position.y, OffsetEnd.transform.position.y));
        int x2 = (int)math.floor(OffsetEnd.transform.position.x);
        int y2 = (int)math.floor(LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).WorldGenerationDataObjectsContainer.GetComponentsInChildren<BuildingRoofedTop>().OrderBy(r => r.OffsetStart.transform.position.y).Last().OffsetStart.transform.position.y);

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
        for (int x = x1; x <= x2; x++)
        {
            for (int y = y2; y <= y2 + RoofHeight; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y);

                if (!generateWhere.GetHasAnyTileAt(tilePos))
                {
                    targetTilemap.SetTile(tilePos, RoofTile);
                }
            }
        }

        Destroy(gameObject);
    }
}
