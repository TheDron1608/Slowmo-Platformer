using NUnit.Framework.Constraints;
using System;
using System.Collections;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    const float SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER = 5f;

    public float SelectMaxRangeMultiplier = 1f; //value between 0 and 1
    public float SelectedColorDarkness = 0.64f; //value between 0 and 256

    private SpriteRenderer _spriteRendererComponent;

    private bool _selected = false;
    public bool Selected
    {
        get => _selected;
        set
        {
            if (value)
            {
                if (!_selected)
                {
                    StartCoroutine(SelectProcess(true));
                }
            }
            else
            {
                if (_selected)
                {
                    StartCoroutine(SelectProcess(false));
                }
            }
            _selected = value;
        }
    }

    private IEnumerator SelectProcess(bool selected)
    {
        float StepMultiplier = (selected ? 1f : -1f) * SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER;

        float colorDarknessProgress = 0f;

        while (colorDarknessProgress < SelectedColorDarkness)
        {
            colorDarknessProgress += Time.deltaTime * SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER;
            _spriteRendererComponent.color = new Color(
                _spriteRendererComponent.color.r - Time.deltaTime * StepMultiplier,
                _spriteRendererComponent.color.g - Time.deltaTime * StepMultiplier,
                _spriteRendererComponent.color.b - Time.deltaTime * StepMultiplier
            );
            yield return new WaitForEndOfFrame();
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _spriteRendererComponent)) throw new UnityException("SpriteRenderer component not found");
    }
}
