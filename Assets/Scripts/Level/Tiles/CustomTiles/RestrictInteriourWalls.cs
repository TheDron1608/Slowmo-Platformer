using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RestrictInteriourWalls", menuName = "2D/Tiles/CustomTiles/RestrictInteriourWalls")]
public class RestrictInteriourWalls : Tile
{
#if UNITY_EDITOR
    [Header("debug option")]
    [SerializeField] private Sprite _debugTileSprite;

    [CustomEditor(typeof(RestrictInteriourWalls))]
    public class ToggleInvisibleTile : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("ToggleInvisibleTile"))
            {
                RestrictInteriourWalls selfTile = target as RestrictInteriourWalls;

                selfTile.sprite = selfTile.sprite == null ? selfTile._debugTileSprite : null;
            }
        }
    }
#endif
}
