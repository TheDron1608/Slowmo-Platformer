using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIElementTrackTarget : MonoBehaviour
{
    const float TRACK_SPEED_MULTIPLIER = 5f;

    public Transform TrackingUIElement;

    private void Update()
    {
        if (TrackingUIElement == null) Destroy(gameObject);
        else TrackingUIElement.transform.position = math.lerp(TrackingUIElement.transform.position, transform.position, Time.deltaTime * TRACK_SPEED_MULTIPLIER);
    }

    public static UIElementTrackTarget CreateTrackTarget(Transform parent, Transform trackingUIElement)
    {
        GameObject newGO = new("TrackTarget_" + trackingUIElement.gameObject.name);
        newGO.transform.parent = parent;

        RectTransform newGORectTransform = newGO.AddComponent<RectTransform>();
        newGORectTransform.sizeDelta = trackingUIElement.GetComponent<RectTransform>().sizeDelta;

        UIElementTrackTarget newGOTrackTarget = newGO.AddComponent<UIElementTrackTarget>();
        newGOTrackTarget.TrackingUIElement = trackingUIElement;

        return newGOTrackTarget;
    }
}