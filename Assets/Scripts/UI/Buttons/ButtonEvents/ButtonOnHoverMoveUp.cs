using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverMoveUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    const float IMAGE_MOVE_UP_DISTANCE = 75f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 5.0f;

    private bool _movingUp = false;
    private Button _buttonComponent;

    private void Start()
    {
        _buttonComponent = GetComponent<Button>();
    }

    private void Update()
    {
        if (_buttonComponent == null || _buttonComponent.interactable)
        {
            if (_movingUp)
            {
                transform.position = math.lerp(transform.position, transform.parent.position + (Vector3.up * IMAGE_MOVE_UP_DISTANCE), IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime);
            }
            else
            {
                transform.position = math.lerp(transform.position, transform.parent.position, IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.unscaledDeltaTime);
            }
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
}
