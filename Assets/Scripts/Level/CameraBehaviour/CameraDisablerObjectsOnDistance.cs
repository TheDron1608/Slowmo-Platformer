using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraDisablerObjectsOnDistance : MonoBehaviour
{
    const int UPDATES_PER_SECOND = 10;

    public List<DisableObjectOnDistanceFromCamera> TrackedObjects = new();

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(UpdateLoop());
    }

    private IEnumerator UpdateLoop()
    {
        while (true)
        {
            UpdateEnabled();
            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
        }
    }

    private void UpdateEnabled()
    {
        foreach (var trackedObject in TrackedObjects)
        {
            if (trackedObject.enabled && trackedObject.DisableCondition())
            {
                bool newValue = Vector2.Distance(transform.position, trackedObject.transform.position) < trackedObject.DistanceToDistable;
                if (trackedObject.gameObject.activeSelf != newValue)
                {
                    trackedObject.gameObject.SetActive(newValue);
                }
            }
        }
    }
}
