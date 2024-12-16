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
        _currentIndex = _startZLayer.GetZLayer();
    }

    public ZIndexLayer GetCurrentZIndexLayer()
    {
        for (int i = 0; i < ZIndexLayer.ZLayers.Count; i++)
        {
            if (ZIndexLayer.ZLayers[i].GetZLayer() == CurrentZIndex) return ZIndexLayer.ZLayers[i];
        }
        throw new UnityException($"ZIndex {CurrentZIndex} not found");
    }
}
