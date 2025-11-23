using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(5)]
public class CharacterPartVisual : AbstractCharacterComponent
{
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private CharacterMultiSpritesSO.AnimatedCharacterParts _visualType;

    public CharacterMultiSpritesSO.AnimatedCharacterParts VisualType
    {
        get => _visualType;
        set
        {
            if (_visualType != value)
            {
                _spriteRenderer.sortingOrder -= CharComponents.CharacterVisual.MultiSpritesSO.AnimatedCharacerPartsOrderInLayer[VisualType];
                _spriteRenderer.sortingOrder += CharComponents.CharacterVisual.MultiSpritesSO.AnimatedCharacerPartsOrderInLayer[VisualType];

                _visualType = value;
            }
        }
    }

    public Material SharedMaterial
    {
        get => _spriteRenderer.sharedMaterial;
        set => _spriteRenderer.sharedMaterial = value;
    }

    public bool IsVisible()
    {
        return _spriteRenderer.isVisible;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found");

        CharComponents.CharacterVisual.OnSampleSpriteChanged += CharacterVisual_OnSampleSpriteChanged;
        CharComponents.CharacterVisual.OnSpriteFlippedChanged += CharacterVisual_OnSpriteFlippedChanged;

        _spriteRenderer.sortingOrder += CharComponents.CharacterVisual.MultiSpritesSO.AnimatedCharacerPartsOrderInLayer[VisualType];
        _spriteRenderer.sortingOrder += CharComponents.CharacterVisual.RandomExtraSpriteRendererSortingOrder;
    }

    private void CharacterVisual_OnSampleSpriteChanged(object sender, Sprite sampleSprite)
    {
        try
        {
            _spriteRenderer.sprite = CharComponents.CharacterVisual.MultiSpritesSO.GetSampleSprites(sampleSprite)[(int)VisualType];
        }
        catch (KeyNotFoundException)
        {
            throw new UnityException(
                "not found sprite in CharacterPartVisualManager.Instance.SampleSprites with key " + sampleSprite +
                " try press UpdateCharacterTextures button in inspector of CharacterPartVisualManager gameObject"
                );
        }
    }

    private void CharacterVisual_OnSpriteFlippedChanged(object sender, bool e)
    {
        _spriteRenderer.flipX = e;
    }

    private void OnDestroy()
    {
        CharComponents.CharacterVisual.OnSampleSpriteChanged -= CharacterVisual_OnSampleSpriteChanged;
        CharComponents.CharacterVisual.OnSpriteFlippedChanged -= CharacterVisual_OnSpriteFlippedChanged;
    }
}
