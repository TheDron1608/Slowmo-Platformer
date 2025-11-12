using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class OnEnableScaleEffect : MonoBehaviour
{
    public float ScaleEffectSpeed = 15f;
    public bool ScaleX = true;
    public bool ScaleY = true;

    private float _scaleEffectProgress = 1.0f;

    private void OnEnable()
    {
        ScaleEffectProgress = 0f;
    }

    private void Update()
    {
        if (ScaleEffectProgress < 1f)
        {
            ScaleEffectProgress = math.lerp(ScaleEffectProgress, 1f, Time.unscaledDeltaTime * ScaleEffectSpeed);
            if (ScaleEffectProgress > 1f ) ScaleEffectProgress = 1f;
        }
    }

    private float ScaleEffectProgress
    {
        get => _scaleEffectProgress;
        set
        {
            _scaleEffectProgress = value;

            transform.localScale = new Vector3(
                ScaleX ? value : transform.localScale.x,
                ScaleY ? value : transform.localScale.y,
                transform.localScale.z
            );
        }
    }
}