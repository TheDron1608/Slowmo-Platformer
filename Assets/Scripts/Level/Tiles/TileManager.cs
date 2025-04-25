using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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

        public void Debug_DrawPlatform(Color color, float duration)
        {
            Debug.DrawLine(new Vector2(Position.x + 0.1f, Position.y + 1.4f), new Vector2(Position.x + Width - 0.1f, Position.y + 1.4f), color, duration);
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

        public void Debug_DrawTransition(Color color, float duration)
        {
            Debug.DrawLine(StartConntection + new Vector2(0.5f, 0.5f), EndConnection + new Vector2(0.5f, 0.5f), color, duration);
            Debug.DrawLine(EndConnection + new Vector2(0.6f, 0.6f), EndConnection + new Vector2(0.4f, 0.4f), color, duration);
            Debug.DrawLine(EndConnection + new Vector2(0.6f, 0.4f), EndConnection + new Vector2(0.4f, 0.6f), color, duration);
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



    public NavigationPlatformTransitionInfo TryCreateTransition(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth, NavigationPlatformDirection prefferedDirection)
    {
        if (
            from.Position.y + maxJumpHeight > to.Position.y &&
            !(from.Position.x - maxJumpWidth < to.Position.x + to.Width ^ from.Position.x + from.Width + maxJumpWidth > to.Position.x)
            )
        {


            if (from.Position.y > to.Position.y)
            {
                NavigationPlatformTransitionInfo result;
                switch (prefferedDirection)
                {
                    case NavigationPlatformDirection.RIGHT:
                        result = TryCreateTransitionDownFromRightPrefferedRight(from, to, maxJumpHeight, maxJumpWidth);
                        if (result != null) return result;
                        result = TryCreateTransitionDownFromLeftPrefferedRight(from, to, maxJumpHeight, maxJumpWidth);
                        return result;
                    case NavigationPlatformDirection.LEFT:
                        result = TryCreateTransitionDownFromLeftPrefferedLeft(from, to, maxJumpHeight, maxJumpWidth);
                        if (result != null) return result;
                        result = TryCreateTransitionDownFromRightPrefferedLeft(from, to, maxJumpHeight, maxJumpWidth);
                        return result;
                    default:
                        throw new UnityException("TileManager.TryCreateTransition prefferedDirection argument can be only NavigationPlatformDirection.RIGHT or NavigationPlatformDirection.LEFT");

                }
            }
            else if (from.Position.y == to.Position.y)
            {
                if (from.Position.x > to.Position.x)
                {
                    return TryCreateTransitionEqualAltitudeToLeft(from, to, maxJumpHeight, maxJumpWidth);
                }
                else
                {
                    return TryCreateTransitionEqualAltitudeToRight(from, to, maxJumpHeight, maxJumpWidth);
                }
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    // i tried to explain this with words but this simple pic is more effecicent than 500 words description

    //              @
    //            |####
    //            |
    //            V --> 
    // ################
    private NavigationPlatformTransitionInfo TryCreateTransitionDownFromLeftPrefferedRight(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x - maxJumpWidth < to.Position.x + to.Width && from.Position.x > to.Position.x)
        {
            NavigationPlatformTransitionInfo result = new();
            for (int x = math.min(from.Position.x - 1, to.Position.x + to.Width - 1); x <= from.Position.x - 1; x++)
            {
                for (int y = from.Position.y + 1; y > to.Position.y; y--)
                {
                    Debug_MarkTile(new Vector2(x, y), Color.red, 999f);
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        return null;
                    }
                }
            }

            result.StartConntection = new Vector2(from.Position.x + from.Width - 1, from.Position.y + 1);
            result.EndConnection = new Vector2(math.min(from.Position.x - 1, to.Position.x + to.Width - 1), to.Position.y + 1);

            result.Debug_DrawTransition(Color.gray, 999f);

            return result;
        }
        else
        {
            return null;
        }
    }

    //              @
    //            /####
    //           /
    // <--      V
    // ################
    private NavigationPlatformTransitionInfo TryCreateTransitionDownFromLeftPrefferedLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x - maxJumpWidth < to.Position.x + to.Width && from.Position.x > to.Position.x)
        {
            bool isValidTransition = true;
            NavigationPlatformTransitionInfo result = null;
            for (int x = math.min(from.Position.x - 1, to.Position.x + to.Width - 1); x >= math.max(from.Position.x - maxJumpWidth, to.Position.x); x--)
            {
                for (int y = from.Position.y; y > to.Position.y; y--)
                {
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        isValidTransition = false;
                        break;
                    }
                }
                if (!isValidTransition)
                {
                    result?.Debug_DrawTransition(Color.gray, 999f);

                    return result;
                }
                result = new();
                result.StartConntection = new Vector2(from.Position.x, from.Position.y + 1);
                result.EndConnection = new Vector2(x, to.Position.y + 1);

            }
            result?.Debug_DrawTransition(Color.gray, 999f);

            return result;
        }
        else
        {
            return null;
        }
    }

    //   @
    // ####|
    //     |
    // <-- V       
    // ################
    private NavigationPlatformTransitionInfo TryCreateTransitionDownFromRightPrefferedLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x + from.Width < to.Position.x + to.Width && from.Position.x + from.Width + maxJumpWidth > to.Position.x)
        {
            NavigationPlatformTransitionInfo result = new();
            for (int x = math.max(from.Position.x + from.Width, to.Position.x); x >= from.Position.x + from.Width; x--)
            {
                for (int y = from.Position.y + 1; y > to.Position.y; y--)
                {
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        return null;
                    }
                }
            }

            result.StartConntection = new Vector2(from.Position.x + from.Width - 1, from.Position.y + 1);
            result.EndConnection = new Vector2(math.max(from.Position.x + from.Width, to.Position.x), to.Position.y + 1);

            result.Debug_DrawTransition(Color.gray, 999f);

            return result;
        }
        else
        {
            return null;
        }
    }

    //   @
    // ####\
    //      \
    //       V      -->
    // ################
    private NavigationPlatformTransitionInfo TryCreateTransitionDownFromRightPrefferedRight(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x + from.Width < to.Position.x + to.Width && from.Position.x + from.Width + maxJumpWidth > to.Position.x)
        {
            bool isValidTransition = true;
            NavigationPlatformTransitionInfo result = null;
            for (int x = math.max(from.Position.x + from.Width, to.Position.x); x < math.min(from.Position.x + from.Width + maxJumpWidth, to.Position.x + to.Width); x++)
            {
                for (int y = from.Position.y; y > to.Position.y; y--)
                {
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        isValidTransition = false;
                        break;
                    }
                }
                if (!isValidTransition)
                {
                    result?.Debug_DrawTransition(Color.gray, 999f);

                    return result;
                }
                result = new();
                result.StartConntection = new Vector2(from.Position.x + from.Width - 1, from.Position.y + 1);
                result.EndConnection = new Vector2(x, to.Position.y + 1);

            }
            result?.Debug_DrawTransition(Color.gray, 999f);

            return result;
        }
        else
        {
            return null;
        }
    }

    //
    //      <------- @
    //#######      #######
    private NavigationPlatformTransitionInfo TryCreateTransitionEqualAltitudeToLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x - maxJumpWidth < to.Position.x + to.Width - 1 && from.Position.x > to.Position.x)
        {
            for (int x = to.Position.x + to.Width; x <= from.Position.x - 1; x++)
            {
                for (int y = from.Position.y + 1; y < from.Position.y + maxJumpHeight; y++)
                {
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        return null;
                    }
                }
            }

            NavigationPlatformTransitionInfo result = new();
            result.StartConntection = new Vector2(from.Position.x, from.Position.y + 1);
            result.EndConnection = new Vector2(to.Position.x + to.Width - 1, to.Position.y + 1);

            result.Debug_DrawTransition(Color.gray, 999f);
            return result;
        }
        else
        {
            return null;
        }
    }

    //
    //    @ ------->
    //#######      #######
    private NavigationPlatformTransitionInfo TryCreateTransitionEqualAltitudeToRight(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x + from.Width - 1 + maxJumpWidth > to.Position.x && from.Position.x < to.Position.x)
        {
            for (int x = from.Position.x + from.Width - 1; x <= to.Position.x; x++)
            {
                for (int y = from.Position.y + 1; y < from.Position.y + maxJumpHeight; y++)
                {
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        return null;
                    }
                }
            }

            NavigationPlatformTransitionInfo result = new();
            result.StartConntection = new Vector2(from.Position.x + from.Width - 1, from.Position.y + 1);
            result.EndConnection = new Vector2(to.Position.x, to.Position.y + 1);

            result.Debug_DrawTransition(Color.gray, 999f);
            return result;
        }
        else
        {
            return null;
        }
    }

    //         ^    -->   
    //         | ######
    //         |
    //         |  @
    // ################
    private NavigationPlatformTransitionInfo TryCreateTransitionUpToRightFromRight(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.y < to.Position.y && from.Position.y + maxJumpHeight > to.Position.y && from.Position.x < to.Position.x)
        {
            NavigationPlatformTransitionInfo result = new();
            int x = math.min(to.Position.x - 1, from.Position.x + from.Width - 1);

            for (int y = from.Position.y + 1; y < to.Position.y + 2; y++)
            {
                Debug_MarkTile(new Vector2(x, y), Color.red, 999f);
                if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                {
                    return null;
                }
            }

            result.StartConntection = new Vector2(x, from.Position.y + 1);
            result.EndConnection = new Vector2(to.Position.x, to.Position.y + 1);

            result.Debug_DrawTransition(Color.gray, 999f);

            return result;
        }
        else
        {
            return null;
        }
    }

    //          ^   -->   
    //         / ######
    //        /
    //    @  /
    // ################
    private NavigationPlatformTransitionInfo TryCreateTransitionUpToRightFromLeft(NavigationPlatformInfo from, NavigationPlatformInfo to, int maxJumpHeight, int maxJumpWidth)
    {
        if (from.Position.x - maxJumpWidth < to.Position.x + to.Width && from.Position.x > to.Position.x)
        {
            bool isValidTransition = true;
            NavigationPlatformTransitionInfo result = null;
            for (int x = math.min(from.Position.x - 1, to.Position.x + to.Width - 1); x >= math.max(from.Position.x - maxJumpWidth, to.Position.x); x--)
            {
                for (int y = from.Position.y; y > to.Position.y; y--)
                {
                    if (GetTileBehaviourAt(new Vector2(x, y)) != null)
                    {
                        isValidTransition = false;
                        break;
                    }
                }
                if (!isValidTransition)
                {
                    result?.Debug_DrawTransition(Color.gray, 999f);

                    return result;
                }
                result = new();
                result.StartConntection = new Vector2(from.Position.x, from.Position.y + 1);
                result.EndConnection = new Vector2(x, to.Position.y + 1);

            }
            result?.Debug_DrawTransition(Color.gray, 999f);

            return result;
        }
        else
        {
            return null;
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

                TryCreateTransition(platform, subPlatform, 5, 4, NavigationPlatformDirection.RIGHT);
            }
        }
    }

    public void Debug_MarkTile(Vector2 tilePos, Color color, float duration)
    {
        Debug.DrawLine(tilePos + new Vector2(0.1f, 0.1f), tilePos + new Vector2(0.9f, 0.9f), color, duration);
        Debug.DrawLine(tilePos + new Vector2(0.1f, 0.9f), tilePos + new Vector2(0.9f, 0.1f), color, duration);
    }
}
