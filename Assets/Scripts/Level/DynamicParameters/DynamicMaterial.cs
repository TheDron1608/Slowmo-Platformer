using System;
using UnityEngine;

public class DynamicMaterial : MonoBehaviour
{
    [SerializeField] private LevelColorset.ColorType _defaultColor;
    private Material _overrideMaterial = null;

    public event EventHandler OnMaterialChanged;

    public LevelColorset.ColorType DefaultColor
    {
        get => _defaultColor;
        set
        {
            _defaultColor = value;
            UpdateColor();
        }
    }
    public Material OverrideMaterial
    {
        get => _overrideMaterial;
        set
        {
            _overrideMaterial = value;
            UpdateColor();
        }
    }

    public Material GetCurrentMaterial()
    {
        return OverrideMaterial ?? ColorManager.Instance.ColorSet.GetMaterialByType(DefaultColor);
    }

    private void UpdateColor()
    {
        if (TryGetComponent(out Renderer renderer))
        {
            if (renderer.sharedMaterial != GetCurrentMaterial())
            {
                renderer.sharedMaterial = GetCurrentMaterial();
                OnMaterialChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Awake()
    {
        UpdateColor();
    }
}
