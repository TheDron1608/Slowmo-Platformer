using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CharacterPartVisual : AbstractCharacterComponent
{
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private CharacterPartVisualManager.AnimatedCharacterParts _visualType;

    public CharacterPartVisualManager.AnimatedCharacterParts VisualType
    {
        get => _visualType;
        set
        {
            if (_visualType != value)
            {
                _spriteRenderer.sortingOrder -= (int)Enum.GetValues(typeof(CharacterPartVisualManager.AnimatedCharacerPartsOrderInLayer)).GetValue((int)_visualType);
                _spriteRenderer.sortingOrder += (int)Enum.GetValues(typeof(CharacterPartVisualManager.AnimatedCharacerPartsOrderInLayer)).GetValue((int)value);

                _visualType = value;
            }
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found");

        CharComponents.CharacterVisual.OnSampleSpriteChanged += CharacterVisual_OnSampleSpriteChanged;
        CharComponents.CharacterVisual.OnSpriteFlippedChanged += CharacterVisual_OnSpriteFlippedChanged;

        _spriteRenderer.sortingOrder += (int)Enum.GetValues(typeof(CharacterPartVisualManager.AnimatedCharacerPartsOrderInLayer)).GetValue((int)VisualType);
    }

    private void CharacterVisual_OnSampleSpriteChanged(object sender, Sprite e)
    {
        _spriteRenderer.sprite = CharacterPartVisualManager.Instance.SampleSprites[e][(int)VisualType];
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
