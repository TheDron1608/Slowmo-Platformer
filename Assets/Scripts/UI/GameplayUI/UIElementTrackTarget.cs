using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIElementTrackTarget : MonoBehaviour
{
    public interface IUIElementTrackTargetable
    {
        public UIElementTrackTarget SelfTrackTarget { get; set; }
    }

    const float TRACK_SPEED_MULTIPLIER = 15f;

    public Transform TrackingUIElement;

    private void Update()
    {
        if (TrackingUIElement == null)
        {
            if (TrackingUIElement is IUIElementTrackTargetable targetableElement)
            {
                targetableElement.SelfTrackTarget = null;
            }
            Destroy(gameObject);
        }
        else
        {
            TrackingUIElement.transform.position = math.lerp(TrackingUIElement.transform.position, transform.position, Time.unscaledDeltaTime * TRACK_SPEED_MULTIPLIER);
        }
    }

    public static UIElementTrackTarget CreateTrackTarget(Transform parent, MonoBehaviour trackingUIElement)
    {
        GameObject newGO = new("TrackTarget_" + trackingUIElement.gameObject.name);

        RectTransform newGORectTransform = newGO.AddComponent<RectTransform>();
        newGORectTransform.sizeDelta = trackingUIElement.GetComponent<RectTransform>().sizeDelta;

        if (trackingUIElement.TryGetComponent(out RectTransform rectTramsform))
        {
            LayoutElement layoutElement = newGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = rectTramsform.rect.height;
            layoutElement.preferredWidth = rectTramsform.rect.width;
        }


        newGO.transform.SetParent(parent);
        newGO.transform.localPosition = Vector3.zero;

        UIElementTrackTarget newGOTrackTarget = newGO.AddComponent<UIElementTrackTarget>();
        newGOTrackTarget.TrackingUIElement = trackingUIElement.transform;

        if (trackingUIElement is IUIElementTrackTargetable targetableElement)
        {
            targetableElement.SelfTrackTarget = newGOTrackTarget;
        }

        return newGOTrackTarget;
    }
}