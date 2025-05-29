using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileBehaviourType
    {
        NORMAL,
        STICKY,
        DOOR,
        BACKGROUND,
        TRANSPARENT_BACKGOUND,
        OVERGOUND
    }

    public TileBehaviourType BehaviourType;
    public bool ValidAsPlatform = true;
}
