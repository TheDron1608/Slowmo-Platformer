using System.Reflection;
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

    public bool ForceDisableLight
    {
        get => _forceDisableLight;
        set
        {
            _forceDisableLight = value;
            _lightComponent.enabled = _lightComponent.intensity > 0 && !ForceDisableLight;
        }
    }

    public float GetDefaultIntensity()
    {
        return _defaultIntensity;
    }

    public void SetLightIntensityMultiplier(float value)
    {
        if (value == 0f)
        {
            _lightComponent.enabled = false;
        }
        else
        {
            _lightComponent.enabled = !ForceDisableLight;
        }
        _lightComponent.intensity = _defaultIntensity * value;
    }

    private void Awake()
    {
        _lightComponent = GetComponent<Light2D>();
        _defaultIntensity = _lightComponent.intensity;
        LightManager.Instance?.AddLightSource(this, LightType);
    }

    private void OnDestroy()
    {
        LightManager.Instance?.RemoveLightSource(this, LightType);
    }
}
