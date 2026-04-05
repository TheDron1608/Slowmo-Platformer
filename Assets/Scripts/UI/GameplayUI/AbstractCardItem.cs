using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class AbstractCardItem : Button, UIElementTrackTarget.IUIElementTrackTargetable
{
    private UIElementTrackTarget _selfTrackTarget;

    public UIElementTrackTarget SelfTrackTarget 
    { 
        get => _selfTrackTarget;
        set => _selfTrackTarget = value; 
    }

    public abstract void Pick();

    public void SetInteractable(bool value)
    {
        GetComponent<ButtonOnHoverMoveUp>().enabled = value;
        GetComponent<ButtonSoundVisualEffects>().enabled = value;
        enabled = value;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (interactable)
        {
            Select();
        }
    }
}