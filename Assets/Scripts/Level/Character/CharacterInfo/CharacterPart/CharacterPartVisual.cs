using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterPartVisual : MonoBehaviour
{
    private Animator _mainAnimator;
    private SpriteRenderer _spriteRenderer;

    public CharacterPartVisualManager.AnimatedCharacterParts VisualType;

    private void Awake()
    {
        _mainAnimator = GetComponent<AbstractCharacterComponent>().CharComponents.Animator;
        if (TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found");
    }

    private void Update()
    {
        AnimatorClipInfo sampleClipInfo = _mainAnimator.GetCurrentAnimatorClipInfo(0)[0];
        //_spriteRenderer.sprite = AnimationUtility.GetObjectReferenceCurveBindings(CharacterPartVisualManager.Instance.GetCharacterPartClip(sampleClipInfo.clip, VisualType))[sampleClipInfo];
    }
}
