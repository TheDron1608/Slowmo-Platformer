using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DynamicMaterial : MonoBehaviour
{
    public bool AllowChangeMaterial = true;

    private Material _defaultMaterial;
    private Material _overrideMaterial = null;

    public event EventHandler OnMaterialChanged;

    public Material DefaultMaterial
    {
        get => _defaultMaterial;
        set
        {
            if (!AllowChangeMaterial) return;

            _defaultMaterial = value;
            UpdateColor();
        }
    }
    public Material OverrideMaterial
    {
        get => _overrideMaterial;
        set
        {
            if (!AllowChangeMaterial) return;

            _overrideMaterial = value;
            UpdateColor();
        }
    }

    public Material GetCurrentMaterial()
    {
        return OverrideMaterial ?? DefaultMaterial;
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
        _defaultMaterial = GetComponent<Renderer>().material;
    }
}
