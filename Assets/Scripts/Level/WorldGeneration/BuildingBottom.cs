using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingBottom : MonoBehaviour, ILateGenerationEnviroment
{
    const int MAP_BOTTOM_TILE_Y_POS = 0;

    public GameObject OffsetStart;
    public GameObject OffsetEnd;
    public TileBase FillTile;
    public TileBehaviour.TileBehaviourType FillTileType;

    public void Generate()
    {
        MultiTileMapsContainer generateWhere = LayerManager.Instance.GetZLayerOfGameObject(gameObject).MultiTileMapsContainer;
        Tilemap targetTilemap = generateWhere.GetTileMapByBehaviourType(FillTileType);

        int x1 = (int)math.floor(OffsetStart.transform.position.x);
        int y1 = MAP_BOTTOM_TILE_Y_POS;
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
