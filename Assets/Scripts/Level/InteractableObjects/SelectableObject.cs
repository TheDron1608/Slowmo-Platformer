using NUnit.Framework.Constraints;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectableObject : MonoBehaviour
{
    public enum SelectableObjectType
    {
        UNINTERACTABLE,
        UNNECESSARY_HOLDABLE,
        UNNECESSARY_INTERACTABLE,
        MELEE_WEAPON,
        RANGE_WEAPON,
        THROW_WEAPON,
        Z_INDEX_DOOR
    }

    const float SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER = 5f;

    /// <summary>
    /// Used to make AI bettwe undertand how to interact with object
    /// </summary>
    public SelectableObjectType ObjectType;

    public InputActionReference PlayerInputToInteract;

    public float SelectMaxRangeMultiplier = 1f; //value between 0 and 1
    public float SelectedColorDarkness = 0.64f; //value between 0 and 256

    [SerializeField]
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

    protected virtual void Awake()
    {
        if (PlayerInputToInteract == null) throw new UnityException("PlayerInputToInteract waws not sat, set it in the instector");

        if (_spriteRendererComponent == null)
        {
            if (!TryGetComponent(out _spriteRendererComponent)) throw new UnityException("SpriteRenderer component not found");
        }
    }
}
