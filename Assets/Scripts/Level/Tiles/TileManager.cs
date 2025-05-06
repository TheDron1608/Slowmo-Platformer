using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public enum NavigationPlatformDirection
    {
        LEFT,
        RIGHT
    }

    public class NavigationPlatformInfo
    {
        public Vector3Int Position;
        public int Width;

        public int TailPositionX
        {
            get => Position.x + Width - 1;
        }

        public Vector3Int TailPosition
        {
            get => new Vector3Int(Position.x + Width - 1, Position.y, Position.z);
        }

        public void Debug_DrawPlatform(Color color, float duration)
        {
            Debug.DrawLine(new Vector2(Position.x + 0.1f, Position.y + 1.4f), new Vector2(Position.x + Width - 0.1f, Position.y + 1.4f), color, duration);
        }

        public bool GetPositionInOnPlatform(Vector2Int position)
        {
            return
                Position.y + 1 == position.y &&
                Position.x <= position.x &&
                Position.x + Width >= position.x;
        }
    }

    public class NavigationPlatformTransitionInfo
    {
        public Vector2Int StartConntection;
        public Vector2Int EndConnection;

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

        public void Debug_DrawTransition(Color color, float duration)
        {
            Debug.DrawLine(StartConntection + new Vector2(0.5f, 0.5f), EndConnection + new Vector2(0.5f, 0.5f), color, duration);
            Debug.DrawLine(EndConnection + new Vector2(0.6f, 0.6f), EndConnection + new Vector2(0.4f, 0.4f), color, duration);
            Debug.DrawLine(EndConnection + new Vector2(0.6f, 0.4f), EndConnection + new Vector2(0.4f, 0.6f), color, duration);
        }
    }

    private Tilemap[] _tilemaps;
    private List<NavigationPlatformInfo> _navigationPlatforms;

    public static Vector2Int PositionToTilePosition(Vector2 position)
    {
        return new Vector2Int((int)math.floor(position.x), (int)math.floor(position.y));
    }

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

    public NavigationPlatformInfo GetNearestReachablePlatform(Vector2 position, int maxJumpHeight, int maxJumpWidth)
    {
        float nearestDistance = float.MaxValue;
        NavigationPlatformInfo result = null;
        for (int i = 0; i < _navigationPlatforms.Count; i++)
        {
            if (
                NavigationPlatforms[i].Position.y + maxJumpHeight > position.y &&
                (NavigationPlatforms[i].Position.x - maxJumpWidth < position.x || NavigationPlatforms[i].TailPositionX + maxJumpWidth > position.x)
                )
            {
                float distance = GetDistanceFromPlatformToPoint(NavigationPlatforms[i], position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    result = NavigationPlatforms[i];

                    if (distance <= 0.1f)
                    {
                        return result;
                    }
                }
            }
        }

        return result;
    }

    public float GetDistanceFromPlatformToPoint(NavigationPlatformInfo platform, Vector2 point)
    {
        return Vector2.Distance(point, new Vector2(GetNearestPlatformPositionToPoint(platform, point).x, platform.Position.y + 1f));
    }

    public NavigationPlatformTransitionInfo TryGetValidJumpTargetPositionFromPlatfromToPoint(Vector2Int startPosition, Vector2Int targetPosition, NavigationPlatformInfo platform, int maxJumpHeight, int maxJumpWidth)
    {
        if (
            startPosition.y + maxJumpHeight >= targetPosition.y &&
            platform.Position.x - maxJumpWidth < targetPosition.x &&
            platform.TailPositionX + maxJumpWidth > targetPosition.x
            )
        {
            int? limitedStartPosX = TryGetValidXPositionAtArea(
                targetPosition,
                startPosition,
                startPosition.x < targetPosition.x ? NavigationPlatformDirection.LEFT : NavigationPlatformDirection.RIGHT,
                null,
                math.max(platform.Position.x, targetPosition.x - maxJumpWidth),
                math.min(platform.TailPositionX, targetPosition.x + maxJumpWidth)
                );

            if (limitedStartPosX.HasValue )
            {
                NavigationPlatformTransitionInfo result = new();
                result.StartConntection = new Vector2Int(limitedStartPosX.Value, startPosition.y);
                result.EndConnection = targetPosition;

                return result;
            }
        }
        return null;
    }

    public Vector3Int GetNearestPlatformPositionToPoint(NavigationPlatformInfo platform, Vector2 point)
    {
        if (point.x < platform.Position.x)
        {
            return platform.Position;
        }
        else if (point.x > platform.TailPositionX)
        {
            return platform.TailPosition;
        }
        else
        {
            return new Vector3Int((int)math.round(point.x), platform.Position.y, platform.Position.z);
        }
    }

    public NavigationPlatformTransitionInfo TryCreateTransition(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        if (
            from != to &&
            (
                from.Position.y > to.Position.y &&
                !(from.Position.x - maxJumpWidth < to.Position.x + to.Width ^ from.Position.x + from.Width + maxJumpWidth > to.Position.x)
            ) ||
            (
                from.Position.y == to.Position.y &&
                !(from.Position.x - maxJumpWidth - 2 < to.Position.x + to.Width ^ from.Position.x + from.Width + maxJumpWidth + 2 > to.Position.x)
            ) ||
            (
                from.Position.y + maxJumpHeight > to.Position.y &&
                !(from.Position.x - 1 <= to.Position.x + to.Width ^ from.Position.x + from.Width + 1 >= to.Position.x)
            )
            )
        {
            NavigationPlatformTransitionInfo result;
            if (from.Position.y > to.Position.y)
            {
                if (math.abs(from.Position.x - currentPosition.x) < math.abs(from.TailPositionX - currentPosition.x))
                {
                    //     .--@
                    //    / ############
                    //   v
                    // ######################
                    result = TryCreateTransitionDownLeft(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                    if (result != null) return result;
                    return TryCreateTransitionDownRight(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                }
                else
                {
                    //               @--.
                    //      ############ \
                    //                    v
                    // ######################
                    result = TryCreateTransitionDownRight(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                    if (result != null) return result;
                    return TryCreateTransitionDownLeft(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                }
            }
            else if (from.Position.y == to.Position.y)
            {
                if (from.Position.x > to.Position.x)
                {
                    //     .----------.
                    //    v            \ @
                    // ####            ####
                    return TryCreateTransitionMiddleLeft(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                }
                else
                {
                    //     .----------.
                    //  @ /            v
                    // ####            ####
                    return TryCreateTransitionMiddleRight(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                }
            }
            else
            {
                if (math.abs(to.Position.x - currentPosition.x) < math.abs(to.TailPositionX - currentPosition.x))
                {
                    //     ^
                    //    / ############
                    // @-'
                    // ######################
                    result = TryCreateTransitionUpRight(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                    if (result != null) return result;
                    return TryCreateTransitionUpLeft(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                }
                else
                {
                    //                  ^
                    //      ############ \
                    //                    '-@
                    // ######################
                    result = TryCreateTransitionUpLeft(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                    if (result != null) return result;
                    return TryCreateTransitionUpRight(from, to, currentPosition, maxJumpHeight, maxJumpWidth);
                }
            }
        }
        else
        {
            return null;
        }
    }

    private NavigationPlatformTransitionInfo TryCreateTransitionDownLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        //     .--@
        //    / ############
        //   v
        // ######################
        if (from.Position.x <= to.Position.x) return null;

        Vector2Int fromPosition = new Vector2Int(from.Position.x, from.Position.y + 1);
        Vector2Int toPosition = new Vector2Int(math.max(to.Position.x, from.Position.x - maxJumpWidth), to.Position.y + 1);
        int? x = TryGetValidXPositionAtArea(fromPosition + Vector2Int.left, toPosition, NavigationPlatformDirection.LEFT, currentPosition, to.Position.x, to.TailPositionX);

        if (x == null) return null;
        toPosition.x = x.Value;

        NavigationPlatformTransitionInfo result = new();
        result.StartConntection = fromPosition;
        result.EndConnection = toPosition;

        return result;
    }
    private NavigationPlatformTransitionInfo TryCreateTransitionDownRight(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        //               @--.
        //      ############ \
        //                    v
        // ######################
        if (from.TailPositionX >= to.TailPositionX) return null;

        Vector2Int fromPosition = new Vector2Int(from.TailPositionX, from.Position.y + 1);
        Vector2Int toPosition = new Vector2Int(math.min(to.TailPositionX, from.TailPositionX + maxJumpWidth), to.Position.y + 1);
        int? x = TryGetValidXPositionAtArea(fromPosition + Vector2Int.right, toPosition, NavigationPlatformDirection.RIGHT, currentPosition, to.Position.x, to.TailPositionX);

        if (x == null) return null;
        toPosition.x = x.Value;

        NavigationPlatformTransitionInfo result = new();
        result.StartConntection = fromPosition;
        result.EndConnection = toPosition;

        return result;
    }
    private NavigationPlatformTransitionInfo TryCreateTransitionMiddleLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        //     .----------.
        //    v            \ @
        // ####            ####

        Vector2Int fromPosition = new Vector2Int(from.Position.x, from.Position.y + 1);
        Vector2Int toPosition = new Vector2Int(to.TailPositionX, to.Position.y + 1);

        if (GetHasUnmovableBlocksInArea(fromPosition, toPosition + Vector2Int.up * (maxJumpHeight - 1))) return null;

        NavigationPlatformTransitionInfo result = new();
        result.StartConntection = fromPosition;
        result.EndConnection = toPosition;

        return result;
    }
    private NavigationPlatformTransitionInfo TryCreateTransitionMiddleRight(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        //     .----------.
        //  @ /            v
        // ####            ####

        Vector2Int fromPosition = new Vector2Int(from.TailPositionX, from.Position.y + 1);
        Vector2Int toPosition = new Vector2Int(to.Position.x, to.Position.y + 1);

        if (GetHasUnmovableBlocksInArea(fromPosition, toPosition + Vector2Int.up * (maxJumpHeight - 1))) return null;

        NavigationPlatformTransitionInfo result = new();
        result.StartConntection = fromPosition;
        result.EndConnection = toPosition;

        return result;
    }
    private NavigationPlatformTransitionInfo TryCreateTransitionUpRight(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        //     ^
        //    / ############
        // @-'
        // ######################
        if (from.Position.x >= to.Position.x) return null;

        Vector2Int fromPosition = new Vector2Int(math.max(from.Position.x, to.Position.x - maxJumpWidth), from.Position.y + 1);
        Vector2Int toPosition = new Vector2Int(to.Position.x, to.Position.y + 1);
        int? x = TryGetValidXPositionAtArea(fromPosition, toPosition + Vector2Int.left, NavigationPlatformDirection.LEFT, currentPosition, from.Position.x, from.TailPositionX);

        if (x == null) return null;
        fromPosition.x = x.Value;

        NavigationPlatformTransitionInfo result = new();
        result.StartConntection = fromPosition;
        result.EndConnection = toPosition;

        return result;
    }
    private NavigationPlatformTransitionInfo TryCreateTransitionUpLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, Vector2Int currentPosition, int maxJumpHeight, int maxJumpWidth)
    {
        //                  ^
        //      ############ \
        //                    '-@
        // ######################
        if (from.TailPositionX <= to.TailPositionX) return null;

        Vector2Int fromPosition = new Vector2Int(math.min(from.TailPositionX, to.TailPositionX + maxJumpWidth), from.Position.y + 1);
        Vector2Int toPosition = new Vector2Int(to.TailPositionX, to.Position.y + 1);
        int? x = TryGetValidXPositionAtArea(fromPosition, toPosition + Vector2Int.right, NavigationPlatformDirection.RIGHT, currentPosition, from.Position.x, from.TailPositionX);

        if (x == null) return null;
        fromPosition.x = x.Value;

        NavigationPlatformTransitionInfo result = new();
        result.StartConntection = fromPosition;
        result.EndConnection = toPosition;

        return result;
    }

    private int? TryGetValidXPositionAtArea(Vector2Int start, Vector2Int end, NavigationPlatformDirection prefferedDirection, Vector2Int? prefferedPosition, int minResult, int maxResult, bool returnFirstValid = false)
    {
        Vector2Int filteredStart = new Vector2Int(math.min(start.x, end.x), math.min(start.y, end.y));
        Vector2Int filteredEnd = new Vector2Int(math.max(start.x, end.x), math.max(start.y, end.y));
        int? result = null;

        for (
            int x = prefferedDirection == NavigationPlatformDirection.RIGHT ? filteredStart.x : filteredEnd.x;
            prefferedDirection == NavigationPlatformDirection.RIGHT ? x <= filteredEnd.x : x >= filteredStart.x;
            x += prefferedDirection == NavigationPlatformDirection.RIGHT ? 1 : -1
            )
        {
            for (int y = filteredStart.y; y <= filteredEnd.y; y++)
            {
                if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                {
                    return result;
                }
            }
            if (x >= minResult && x <= maxResult)
            {
                result = x;
                if (prefferedPosition.HasValue && x == prefferedPosition.Value.x || returnFirstValid) return result;
            }
        }

        return result;
    }
    private bool GetHasUnmovableBlocksInArea(Vector2Int start, Vector2Int end)
    {
        Vector2Int filteredStart = new Vector2Int(math.min(start.x, end.x), math.min(start.y, end.y));
        Vector2Int filteredEnd = new Vector2Int(math.max(start.x, end.x), math.max(start.y, end.y));

        for (int x = filteredStart.x; x <= filteredEnd.x; x++)
        {
            for (int y = filteredStart.y; y <= filteredEnd.y; y++)
            {
                if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Debug_DrawAINavigationPaths(Color color, float duration, int maxJumpHeight, int maxJumpWidth)
    {
        foreach (NavigationPlatformInfo platform in _navigationPlatforms)
        {
            platform.Debug_DrawPlatform(color, duration);

            foreach (NavigationPlatformInfo subPlatform in _navigationPlatforms)
            {
                if (subPlatform == platform) continue;

                TryCreateTransition(platform, subPlatform, new Vector2Int(platform.TailPositionX, platform.Position.y + 1), maxJumpHeight, maxJumpWidth)?.Debug_DrawTransition(color, duration);
                TryCreateTransition(platform, subPlatform, new Vector2Int(platform.TailPositionX, platform.Position.y + 1), maxJumpHeight, maxJumpWidth)?.Debug_DrawTransition(color, duration);
            }
        }
    }

    public void Debug_MarkTile(Vector2 tilePos, Color color, float duration)
    {
        Debug.DrawLine(tilePos + new Vector2(0.1f, 0.1f), tilePos + new Vector2(0.9f, 0.9f), color, duration);
        Debug.DrawLine(tilePos + new Vector2(0.1f, 0.9f), tilePos + new Vector2(0.9f, 0.1f), color, duration);
    }

    public void Debug_MarkArea(Vector2 from, Vector2 to, Color color, float duration)
    {
        Vector2 filteredStart = new Vector2(math.min(from.x, to.x), math.min(from.y, to.y));
        Vector2 filteredEnd = new Vector2(math.max(from.x, to.x), math.max(from.y, to.y));
        Debug.DrawLine(new Vector2(filteredStart.x + 0.1f, filteredStart.y + 0.1f), new Vector2(filteredStart.x + 0.1f, filteredEnd.y + 0.9f), color, duration);
        Debug.DrawLine(new Vector2(filteredStart.x + 0.1f, filteredEnd.y + 0.9f), new Vector2(filteredEnd.x + 0.9f, filteredEnd.y + 0.9f), color, duration);
        Debug.DrawLine(new Vector2(filteredEnd.x + 0.9f, filteredEnd.y + 0.9f), new Vector2(filteredEnd.x + 0.9f, filteredStart.y + 0.1f), color, duration);
        Debug.DrawLine(new Vector2(filteredEnd.x + 0.9f, filteredStart.y + 0.1f), new Vector2(filteredStart.x + 0.1f, filteredStart.y + 0.1f), color, duration);
    }
}
