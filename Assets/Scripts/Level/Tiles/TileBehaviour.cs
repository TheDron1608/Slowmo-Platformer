using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileBehaviourType
    {
        NORMAL,
        STICKY,
        DOOR,
        BACKGROUND,
        WINDOWS,
        BACKGROUND_DECORATIONS,
        OVERGROUND,
        OVERGROUND_DECORATIONS,
        CHUNK_MASK
    }

    public TileBehaviourType BehaviourType;
    /// <summary>
    /// If false, AI pathfinding will ignore this tilemap
    /// </summary>
    public bool ValidAsPlatform = true;
    /// <summary>
    /// If true, new added chunks will be removed if OverrideOrder is less or equal, else will not be added
    /// </summary>
    public bool Overridable = false;
    /// <summary>
    /// If Overridable is true, will remove all other tiles on this coordinate for every overridable tilemaps
    /// </summary>
    public int OverrideOrder = 0;
}
