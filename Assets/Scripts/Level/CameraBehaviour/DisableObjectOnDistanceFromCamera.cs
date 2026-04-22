using UnityEngine;

[DefaultExecutionOrder(100)]
public class DisableObjectOnDistanceFromCamera : MonoBehaviour
{
    public float DistanceToDistable = 50f;

    private void Awake()
    {
        Camera.main.GetComponent<CameraDisablerObjectsOnDistance>().TrackedObjects.Add(this);
    }

    private void OnDestroy()
    {
        Camera.main?.GetComponent<CameraDisablerObjectsOnDistance>().TrackedObjects.Remove(this);
    }
}
