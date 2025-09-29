using UnityEngine;
using UnityEngine.Tilemaps;

public class DynamicMaterial : MonoBehaviour
{
    [SerializeField] private LevelColorset.ColorType _defaultColor;
    private LevelColorset.ColorType? _overrideColor = null;

    public LevelColorset.ColorType DefaultColor
    {
        get => _defaultColor;
        set
        {
            _defaultColor = value;
            UpdateColor();
        }
    }
    public LevelColorset.ColorType? OverrideColor
    {
        get => _overrideColor;
        set
        {
            _overrideColor = value;
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        Material targetMaterial = 
            OverrideColor.HasValue ?
            ColorManager.Instance.ColorSet.GetMaterialByType(OverrideColor.Value) :
            ColorManager.Instance.ColorSet.GetMaterialByType(DefaultColor);

        if (TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.sharedMaterial = targetMaterial;
        }
        else if (TryGetComponent(out TilemapRenderer tilemapRenderer))
        {
            tilemapRenderer.sharedMaterial = targetMaterial;
        }
        else if (TryGetComponent(out ParticleSystemRenderer particleSystem))
        {
            particleSystem.sharedMaterial = targetMaterial;
        }
    }

    private void Awake()
    {
        UpdateColor();
    }
}
