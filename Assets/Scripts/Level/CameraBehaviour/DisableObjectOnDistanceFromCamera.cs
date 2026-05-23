using UnityEngine;

[DefaultExecutionOrder(100)]
public class DisableObjectOnDistanceFromCamera : MonoBehaviour
{
    public float DistanceToDistable = 50f;
    public bool DisableOnDifferentLayers = true;

    public bool ForceDisable
    {
        get => enabled;
        set
        {
            enabled = !value;
            if (value) gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        Camera.main.GetComponent<CameraDisablerObjectsOnDistance>().TrackedObjects.Add(this);
    }

    private void OnDestroy()
    {
        Camera.main?.GetComponent<CameraDisablerObjectsOnDistance>().TrackedObjects.Remove(this);
    }

    public virtual bool DisableCondition()
    {
        return true;
    }
}
