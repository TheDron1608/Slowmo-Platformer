using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RestrictInteriourWalls", menuName = "2D/Tiles/CustomTiles/RestrictInteriourWalls")]
public class RestrictInteriourWalls : Tile
{
    private void Awake()
    {
        sprite = null;
    }
}
