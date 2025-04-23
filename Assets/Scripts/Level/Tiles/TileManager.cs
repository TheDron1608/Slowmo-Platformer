using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    struct NavigationPlatformInfo
    {
        public Vector3Int Position;
        public int Width;
    }

    private Tilemap[] _tilemaps;
    private List<NavigationPlatformInfo> _navigationPlatforms;

    private void Awake()
    {
        _tilemaps = GetComponentsInChildren<Tilemap>();
        UpdateTileAINavigationInfo();
    }

    public TileBehaviour.TileBehaviourType? GetTileBehaviourAt(Vector2 position)
    {
        foreach (Tilemap tilemap in _tilemaps)
        {
            if (tilemap.HasTile(
                new Vector3Int(
                    (int)math.floor(position.x), 
                    (int)math.floor(position.y)
                    )
                ))
            {
                return tilemap.GetComponent<TileBehaviour>().BehaviourType;
            }
        }
        return null;
    }

    public void UpdateTileAINavigationInfo()
    {
        _navigationPlatforms = new();

        foreach (Tilemap tilemap in _tilemaps)
        {
            NavigationPlatformInfo currentPlatform = new();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos) && !tilemap.HasTile(pos + Vector3Int.up))
                {
                    //Debug.DrawLine(new Vector2(pos.x + 0.25f, pos.y + 0.25f), new Vector2(pos.x + 0.75f, pos.y + 0.75f), Color.red, 999f);
                    //Debug.DrawLine(new Vector2(pos.x + 0.75f, pos.y + 0.25f), new Vector2(pos.x + 0.25f, pos.y + 0.75f), Color.red, 999f);

                    if (currentPlatform.Width == 0)
                    {
                        currentPlatform.Position = pos;
                        currentPlatform.Width = 1;
                    }
                    else
                    {
                        currentPlatform.Width++;
                    }
                }
                else
                {
                    Debug.DrawLine(
                        new Vector2(currentPlatform.Position.x + 0.1f, currentPlatform.Position.y + 1.1f),
                        new Vector2(currentPlatform.Position.x - 0.1f + currentPlatform.Width, currentPlatform.Position.y + 1.1f),
                        Color.green, 
                        999f
                        );

                    _navigationPlatforms.Add(currentPlatform);
                    currentPlatform = new();
                }
            }
        }
    }
}
