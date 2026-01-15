using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileBehaviourType
    {
        FOREBGROUND,
        BACKGROUND,
        WINDOWS,
        BACKGROUND_DECORATIONS,
        OVERGROUND,
        OVERGROUND_DECORATIONS,
        CHUNK_MASK,
        HALLUCINATION_TILES,
        OVERGROUND_HALLUCINATION_TILES
    }

    public TileBehaviourType BehaviourType;
}
