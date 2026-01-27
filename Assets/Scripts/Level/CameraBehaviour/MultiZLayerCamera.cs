using Unity.Mathematics;
using UnityEngine;

public class MultiZLayerCamera : MonoBehaviour
{
    public float ZoomOutDistance = 15f;
    public float LayerAppearDistance = 1.5f;
    public float OvergoundAppearOffset = 4f;

    [SerializeField]
    private ZIndexLayer _startZLayer;

    private Camera _cameraComponent;

    private ZIndexLayer _currentLayer;
    public ZIndexLayer CurrentZLayer
    {
        get => _currentLayer;
        private set
        {
            _currentLayer = value;
        }
    }

    private void Awake()
    {
        _currentLayer = _startZLayer;
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
                NumberMath.LimitFloatBetweenZeroAndOne((distanceToLayer - OvergoundAppearOffset) / LayerAppearDistance),
                NumberMath.LimitFloatBetweenZeroAndOne(math.min(1f - (distanceToLayer - OvergoundAppearOffset) / LayerAppearDistance, distanceToLayer / LayerAppearDistance))
                );
            //Debug.Log(LayerManager.Instance.ZLayers[i].ZIndex + " : " + LayerManager.Instance.ZLayers[i].LayerAlpha.Alpha + ", " + LayerManager.Instance.ZLayers[i].LayerAlpha.OvergoundAlpha);
        }
    }

    private void FixedUpdate()
    {
        CurrentZLayer = GetCurrentZIndexLayer();
    }

    private ZIndexLayer GetCurrentZIndexLayer()
    {
        for (int i = LayerManager.Instance.ZLayers.Count - 1; i > 0; i--)
        {
            if (transform.position.z + LayerAppearDistance < LayerManager.Instance.ZLayers[i].transform.position.z) return LayerManager.Instance.ZLayers[i];
        }
        return null;
    }
}
