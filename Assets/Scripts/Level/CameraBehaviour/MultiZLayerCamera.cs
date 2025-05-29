using UnityEngine;

public class MultiZLayerCamera : MonoBehaviour
{
    public float ZoomOutDistance = 15f;
    public float LayerAppearDistance = 1.5f;
    public float OvergoundAppearOffset = 2f;

    [SerializeField]
    private ZIndexLayer _startZLayer;

    private Camera _cameraComponent;

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
                GetCurrentZIndexLayer().transform.position.z - ZoomOutDistance
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
        if (!TryGetComponent(out _cameraComponent)) throw new UnityException("Camera component not found");
    }

    private void LateUpdate()
    {
        UpdateLayerAlpha();
    }

    private void UpdateLayerAlpha()
    {
        for (int i = 0; i < LayerManager.Instance.ZLayers.Count; i++)
        {
            float distanceToLayer = LayerManager.Instance.ZLayers[i].transform.position.z - transform.position.z - _cameraComponent.nearClipPlane;

            LayerManager.Instance.ZLayers[i].LayerAlpha = new(
                NumberMath.LimitFloatBetweenZeroAndOne(distanceToLayer / LayerAppearDistance),
                NumberMath.LimitFloatBetweenZeroAndOne(distanceToLayer - OvergoundAppearOffset / LayerAppearDistance)
                );
        }
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
