using UnityEngine;

[DefaultExecutionOrder(100)]
public class DisableMarkedObjectsOnDistance : MonoBehaviour
{
    private void FixedUpdate()
    {
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (DisableObjectOnDistanceMark disableObjects in layer.GetComponentsInChildren<DisableObjectOnDistanceMark>(true))
            {
                disableObjects.gameObject.SetActive(Vector2.Distance(transform.position, disableObjects.transform.position) < disableObjects.DistanceToDistable);
            }
        }
    }
}
