using UnityEngine;

public class LayerSpriteCustomVisibility : MonoBehaviour
{
    public enum LayerSpriteCustomVisibilityTypes
    {
        OVERGROUND,
        HIDE_ON_OVERGROUNDED,
        DEFAULT
    }

    public LayerSpriteCustomVisibilityTypes VisibilityType;
}
