using UnityEngine;

[DefaultExecutionOrder(100)]
public class DisableObjectOnDistanceFromCamera : MonoBehaviour
{
    public float DistanceToDistable = 50f;
    public bool DisableOnDistance = true;
    public bool DisableOnDifferentLayers = true;
    private bool _allowDisable = true;

    public bool AllowDisable
    {
        get => _allowDisable;
        set => _allowDisable = value;
    }

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
        if (Camera.main != null && Camera.main.TryGetComponent(out CameraDisablerObjectsOnDistance disabler))
        {
            disabler.TrackedObjects.Remove(this);
        }
    }

    public virtual bool DisableCondition()
    {
        return _allowDisable;
    }
}
