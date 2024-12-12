using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileBehaviourType
    {
        NORMAL,
        STICKY,
        SLIPPERY,
        KILL_ON_TOUCH,
        INSTANT_KILL_ON_TOUCH
    }

    public TileBehaviourType BehaviourType;
}
