using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;

public class CharacterPartVisual : AbstractCharacterComponent
{
    private SpriteRenderer _spriteRenderer;

    public CharacterPartVisualManager.AnimatedCharacterParts VisualType;

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found");

        CharComponents.CharacterVisual.OnSampleSpriteChanged += CharacterVisual_OnSampleSpriteChanged;
        CharComponents.CharacterVisual.OnSpriteFlippedChanged += CharacterVisual_OnSpriteFlippedChanged;
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
    }
}
