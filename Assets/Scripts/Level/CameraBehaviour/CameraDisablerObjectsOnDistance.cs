using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraDisablerObjectsOnDistance : MonoBehaviour
{
    public List<DisableObjectOnDistanceFromCamera> TrackedObjects = new();
    private void FixedUpdate()
    {
        foreach (var trackedObject in TrackedObjects)
        {
            trackedObject.gameObject.SetActive(Vector2.Distance(transform.position, trackedObject.transform.position) < trackedObject.DistanceToDistable);
        }
    }
}
