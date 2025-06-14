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
        OVERGOUND,
        OVERGROUND_DECORATIONS,
        CHUNK_MASK
    }

    public TileBehaviourType BehaviourType;
    /// <summary>
    /// If false, AI pathfinding will ignore this tilemap
    /// </summary>
    public bool ValidAsPlatform = true;
    /// <summary>
    /// If ValidAsPlatform is true, will remove all other tiles on this coordinate for every ValidAsPlatform tilemaps
    /// </summary>
    public int OverrideOrder = 0;
}
