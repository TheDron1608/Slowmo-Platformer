using System.Collections;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    const float SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER = 5f;
    const float SELECTED_COLOR_DARKNESS = 0.64f;

    [Header("Selectable")]
    /// <summary>
    /// Used to make AI bettwe undertand how to interact with object
    /// </summary>

    public float SelectMaxRangeMultiplier = 1f; //value between 0 and 1

    protected SpriteRenderer _spriteRendererComponent;

    private Coroutine SelectProcessCoroutine = null;
    private float _currentDarknessProgress = 0f;


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
                    if (SelectProcessCoroutine != null)
                    {
                        StopCoroutine(SelectProcessCoroutine);
                    }
                    SelectProcessCoroutine = StartCoroutine(SelectProcess(true));
                }
            }
            else
            {
                if (_selected)
                {
                    if (SelectProcessCoroutine != null)
                    {
                        StopCoroutine(SelectProcessCoroutine);
                    }
                    SelectProcessCoroutine = StartCoroutine(SelectProcess(false));
                }
            }
            _selected = value;
        }
    }

    private IEnumerator SelectProcess(bool selected)
    {
        float StepMultiplier = (selected ? 1f : -1f) * SELECTED_COLOR_CHANGE_SPEED_MULTIPLIER;

        while (selected ? _currentDarknessProgress < SELECTED_COLOR_DARKNESS : _currentDarknessProgress > 0f)
        {
            _currentDarknessProgress += Time.deltaTime * StepMultiplier;
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
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (!TryGetComponent(out _spriteRendererComponent)) throw new UnityException("SpriteRenderer component not found");
    }
}
