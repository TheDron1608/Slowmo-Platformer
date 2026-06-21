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

    public virtual void Pick()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            if (container.CardPickInfo.ContainsKey(this))
            {
                container.CardPickInfo[this] = true;
            }
        }
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (interactable)
        {
            Select();
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (!CurrentDeviceTracker.GetGamepadIsConnected())
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);

        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.SetDefaultDisplayedInfo();
        }
    }
}