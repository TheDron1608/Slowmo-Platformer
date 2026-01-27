using UnityEngine;

[RequireComponent(typeof(Renderer))]
[DefaultExecutionOrder(-10)]
public class OverrideRendererEnabled : MonoBehaviour
{
    private Renderer _rendererComponent;

    private void Awake()
    {
        _rendererComponent = GetComponent<Renderer>();
        _overridedValue = _rendererComponent.enabled;

        switch (_serializableOverrideValue)
        {
            case SERIALIZABLE_OVERRIDE_VALUE.NO_OVERRIDE:
                _overrideValue = null;
                break;
            case SERIALIZABLE_OVERRIDE_VALUE.ENABLED:
                _overrideValue = true;
                break;
            case SERIALIZABLE_OVERRIDE_VALUE.DISABLED:
                _overrideValue = false;
                break;
        }
    }

    public enum SERIALIZABLE_OVERRIDE_VALUE
    {
        NO_OVERRIDE,
        ENABLED,
        DISABLED
    }

    [SerializeField] private SERIALIZABLE_OVERRIDE_VALUE _serializableOverrideValue = SERIALIZABLE_OVERRIDE_VALUE.NO_OVERRIDE;

    private bool? _overrideValue = null;
    private bool _overridedValue;

    public bool? OverrideValue
    {
        get => _overrideValue;
        set
        {
            if (_overrideValue == value) return;
            _overrideValue = value;
            TryUpdateEnabled(_overridedValue);
        }
    }

    public bool TryUpdateEnabled(bool enabled)
    {
        _overridedValue = enabled;
        if (_rendererComponent.enabled != OverrideValue.GetValueOrDefault(enabled))
        {
            _rendererComponent.enabled = OverrideValue.GetValueOrDefault(enabled);
        }
        return OverrideValue == null;
    }
}
