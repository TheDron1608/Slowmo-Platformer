using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
[DefaultExecutionOrder(-21)]
public class LightManagerManagedLightSource : MonoBehaviour
{
    public LightManager.LightManagedType LightType;

    [SerializeField] private bool _forceDisableLight = false;
    private Light2D _lightComponent;
    private float _defaultIntensity;
    private float _lightIntensityMult = 1f;
    private float _furnitureDynamicIntensityMult = 1f;

    public bool ForceDisableLight
    {
        get => _forceDisableLight;
        set
        {
            _forceDisableLight = value;
            _lightComponent.enabled = _lightComponent.intensity > 0 && !ForceDisableLight;
        }
    }

    public void SetLightIntensityMultiplier(float value)
    {
        _lightIntensityMult = value;
    }

    private void Awake()
    {
        _lightComponent = GetComponent<Light2D>();
        _defaultIntensity = _lightComponent.intensity;
        LightManager.Instance?.AddLightSource(this, LightType);
    }

    private void Update()
    {
        if (LightType == LightManager.LightManagedType.FURNITURE)
        {
            _furnitureDynamicIntensityMult = 1f - NumberMath.LimitFloatBetweenZeroAndOne(math.unlerp(
                LightManager.Instance.FurnitureDynmicIntensityMinDistance, 
                LightManager.Instance.FurnitureDynmicIntensityMaxDistance,
                Vector2.Distance(Camera.main.transform.position, transform.position)
                ));
        }
        else
        {
            _furnitureDynamicIntensityMult = 1f;
        }

        float targetIntensity = _defaultIntensity * _furnitureDynamicIntensityMult * _lightIntensityMult;

        if (targetIntensity <= 0f)
        {
            if (_lightComponent.enabled) _lightComponent.enabled = false;
        }
        else
        {
            if (_lightComponent.intensity != targetIntensity) _lightComponent.intensity = targetIntensity;
            if (!_lightComponent.enabled) _lightComponent.enabled = true;
        }
    }

    private void OnDestroy()
    {
        LightManager.Instance?.RemoveLightSource(this, LightType);
    }
}
