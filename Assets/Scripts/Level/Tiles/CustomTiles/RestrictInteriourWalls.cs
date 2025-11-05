using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RestrictInteriourWalls", menuName = "2D/Tiles/CustomTiles/RestrictInteriourWalls")]
public class RestrictInteriourWalls : Tile
{
    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        if (!Application.isEditor)
        {
            sprite = null;
        }
        return base.StartUp(position, tilemap, go);
    }
}
