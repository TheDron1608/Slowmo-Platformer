using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSoundVisualEffects : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    const float IMAGE_ON_HOVER_SCALE_MULTIPLIER = 1.1f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 15.0f;

    public AbstractSoundPlayer SoundOnClick;
    public AbstractSoundPlayer SoundOnHoverSelect;

    private bool _scalingUp = false;
    private bool _isFirstFrame = true;
    private Selectable _selectableComponent;

    protected void Awake()
    {
        if (!TryGetComponent(out _selectableComponent)) throw new UnityException("Selectable component not found");

        if (TryGetComponent(out Button button))
        {
            button.onClick.AddListener(() => SoundOnClick.PlaySound());
        }
    }

    protected virtual bool SelectCondition()
    {
        return !_isFirstFrame && _selectableComponent.interactable;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!SelectCondition()) return;
        SoundOnHoverSelect.PlaySound();
        _scalingUp = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _scalingUp = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!SelectCondition()) return;
        SoundOnHoverSelect.PlaySound();
        _scalingUp = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _scalingUp = false;
    }

    private void Update()
    {
        if (_scalingUp)
        {
            float newScale = Mathf.Lerp(transform.localScale.x, IMAGE_ON_HOVER_SCALE_MULTIPLIER, IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
        else
        {
            float newScale = Mathf.Lerp(transform.localScale.x, 1f, IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }

    private void Start()
    {
        _isFirstFrame = false;
    }
}
