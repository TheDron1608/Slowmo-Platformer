using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIElementTrackTarget : MonoBehaviour
{
    const float TRACK_SPEED_MULTIPLIER = 15f;

    public Transform TrackingUIElement;

    private void Update()
    {
        if (TrackingUIElement == null)
        {
            Destroy(gameObject);
        }
        else
        {
            TrackingUIElement.transform.position = math.lerp(TrackingUIElement.transform.position, transform.position, Time.unscaledDeltaTime * TRACK_SPEED_MULTIPLIER);
            TrackingUIElement.transform.localScale = Vector3.one;
        }
    }

    public static UIElementTrackTarget CreateTrackTarget(Transform parent, Transform trackingUIElement)
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
        newGOTrackTarget.TrackingUIElement = trackingUIElement;

        return newGOTrackTarget;
    }
}