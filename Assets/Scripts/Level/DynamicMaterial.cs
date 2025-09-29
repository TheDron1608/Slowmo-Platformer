using System;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        if (TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            if (spriteRenderer.sharedMaterial != GetCurrentMaterial()) 
            {
                spriteRenderer.sharedMaterial = GetCurrentMaterial();
                OnMaterialChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (TryGetComponent(out TilemapRenderer tilemapRenderer))
        {
            if (tilemapRenderer.sharedMaterial != GetCurrentMaterial())
            {
                tilemapRenderer.sharedMaterial = GetCurrentMaterial();
                OnMaterialChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (TryGetComponent(out ParticleSystemRenderer particleSystem))
        {
            if (particleSystem.sharedMaterial != GetCurrentMaterial())
            {
                particleSystem.sharedMaterial = GetCurrentMaterial();
                OnMaterialChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Awake()
    {
        UpdateColor();
    }
}
