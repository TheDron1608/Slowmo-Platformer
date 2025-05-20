using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileBehaviourType
    {
        NORMAL,
        STICKY,
        BACKGROUND,
        TRANSPARENT_BACKGOUND
    }

    public TileBehaviourType BehaviourType;
    public bool ValidAsPlatform = true;
}
