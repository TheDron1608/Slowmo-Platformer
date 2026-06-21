using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverMoveUp : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    const float IMAGE_MOVE_UP_DISTANCE = 5f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 5.0f;

    [SerializeField] private Transform _targetTransform;

    private bool _movingUp = false;
    private Selectable _selectableComponent;

    private void Awake()
    {
        if (!TryGetComponent(out _selectableComponent)) throw new UnityException("Selectable component not found");
    }

    private void Update()
    {
        _targetTransform.position = math.lerp(
            _targetTransform.position, 
            transform.position + (_movingUp ? Vector3.up * IMAGE_MOVE_UP_DISTANCE : Vector3.zero),
            IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_selectableComponent.interactable)
        {
            _movingUp = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _movingUp = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_selectableComponent.interactable)
        {
            _movingUp = true;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _movingUp = false;
    }
}
