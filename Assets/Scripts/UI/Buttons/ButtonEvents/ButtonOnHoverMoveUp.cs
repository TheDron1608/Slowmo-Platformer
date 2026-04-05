using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverMoveUp : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    const float IMAGE_MOVE_UP_DISTANCE = 25f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 5.0f;

    [SerializeField] private Transform _targetTransform;

    private bool _movingUp = false;

    private void Update()
    {
        if (_movingUp)
        {
            _targetTransform.position = math.lerp(_targetTransform.position, transform.position + (Vector3.up * IMAGE_MOVE_UP_DISTANCE), IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime);
        }
        else
        {
            _targetTransform.position = math.lerp(_targetTransform.position, transform.position, IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _movingUp = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _movingUp = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _movingUp = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _movingUp = false;
    }
}
