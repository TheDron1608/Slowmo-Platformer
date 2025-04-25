using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TileManager : MonoBehaviour
{
    public class NavigationPlatformInfo
    {
        public Vector3Int Position;
        public int Width;

        public void Debug_DrawPlatform(Color color, float duration)
        {
            Debug.DrawLine(new Vector2(Position.x + 0.1f, Position.y + 1.1f), new Vector2(Position.x + Width - 0.1f, Position.y + 1.1f), color, duration);
        }

        public bool GetIsUnderVector(Vector2 vector)
        {
            return
                Position.y <= vector.y &&
                Position.x <= vector.x &&
                Position.x + Width >= vector.x;
        }
    }

    public class NavigationPlatformTransitionInfo
    {
        public Vector2 StartConntection;
        public Vector2 EndConnection;

        public static NavigationPlatformTransitionInfo TryCreateNavigationTransition(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
        {
            if (
                from.Position.y > to.Position.y - maxJumpHeight &&
                (from.Position.x < to.Position.x - maxJumpWidth || from.Position.x + from.Width > to.Position.x + from.Width + maxJumpWidth)
                )
            {
                NavigationPlatformTransitionInfo result = new();
                return result;
            }
            else
            {
                return null;
            }
        }

        public static bool GetTransitionIsPossible(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
        {
            if (
                from.Position.y + maxJumpHeight > to.Position.y &&
                !(from.Position.x - maxJumpWidth < to.Position.x + to.Width ^ from.Position.x + from.Width + maxJumpWidth > to.Position.x)
                )
            {
                if (from.Position.y >= to.Position.y)
                {
                    Debug.DrawLine(new Vector2(from.Position.x + 0.5f, from.Position.y + 1.5f), new Vector2(to.Position.x + 0.5f, to.Position.y + 1.5f), Color.white, 999f);
                    Debug.DrawLine(new Vector2(from.Position.x - 0.5f + from.Width, from.Position.y + 1.5f), new Vector2(to.Position.x - 0.5f + to.Width, to.Position.y + 1.5f), Color.white, 999f);
                }
                else
                {

                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private Tilemap[] _tilemaps;
    private List<NavigationPlatformInfo> _navigationPlatforms;

    public List<NavigationPlatformInfo> NavigationPlatforms
    {
        get => _navigationPlatforms;
    }

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
            NavigationPlatformInfo currentPlatform = null;
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos) && !tilemap.HasTile(pos + Vector3Int.up))
                {
                    if (currentPlatform == null)
                    {
                        currentPlatform = new();
                        currentPlatform.Position = pos;
                        currentPlatform.Width = 1;
                    }
                    else
                    {
                        currentPlatform.Width++;
                    }
                }
                else if (currentPlatform != null)
                {
                    //currentPlatform.Debug_DrawPlatform(Color.white, 999f);

                    _navigationPlatforms.Add(currentPlatform);
                    currentPlatform = null;
                }
            }
        }
    }

    private void Start()
    {
        Debug_DrawAINavigationPaths();
    }

    public void Debug_DrawAINavigationPaths()
    {
        foreach (NavigationPlatformInfo platform in _navigationPlatforms)
        {
            platform.Debug_DrawPlatform(Color.white, 999f);

            foreach (NavigationPlatformInfo subPlatform in _navigationPlatforms)
            {
                if (subPlatform == platform) continue;

                NavigationPlatformTransitionInfo.GetTransitionIsPossible(platform, subPlatform, 3, 4);
            }
        }
    }
}
