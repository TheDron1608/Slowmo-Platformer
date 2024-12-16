using UnityEngine;

public class MultiZLayerCamera : MonoBehaviour
{
    const float ZOOM_LAYER_DISTANCE = 15f;

    [SerializeField]
    private ZIndexLayer _startZLayer;

    private int _currentIndex = 1;
    public int CurrentZIndex
    {
        get => _currentIndex;
        set
        {
            _currentIndex = value;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                GetCurrentZIndexLayer().transform.position.z - ZOOM_LAYER_DISTANCE
                );

            //for (int i = 0; i < ZIndexLayer.ZLayers.Count; i++)
            //{
            //    ZIndexLayer.ZLayers[i].gameObject.SetActive(ZIndexLayer.ZLayers[i].GetZLayer() == CurrentZIndex);
            //}
        }
    }

    private void Awake()
    {
        _currentIndex = _startZLayer.ZIndex;
    }

    public ZIndexLayer GetCurrentZIndexLayer()
    {
        for (int i = 0; i < LayerManager.Instance.ZLayers.Count; i++)
        {
            if (LayerManager.Instance.ZLayers[i].ZIndex == CurrentZIndex) return LayerManager.Instance.ZLayers[i];
        }
        throw new UnityException($"ZIndex {CurrentZIndex} not found");
    }
}
