using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverScaleUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    const float IMAGE_ON_HOVER_SCALE_MULTIPLIER = 1.1f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 15.0f;

    private bool _scalingUp = false;
    private Button _buttonComponent;

    private void Start()
    {
        _buttonComponent = GetComponent<Button>();
    }

    private void Update()
    {
        if (_buttonComponent.interactable)
        {
            if (_scalingUp)
            {
                float newScale = Mathf.LerpUnclamped(transform.localScale.x, IMAGE_ON_HOVER_SCALE_MULTIPLIER, IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.deltaTime);
                transform.localScale = new Vector3(newScale, newScale, newScale);
            }
            else
            {
                float newScale = Mathf.LerpUnclamped(transform.localScale.x, 1f, IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER * Time.deltaTime);
                transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _scalingUp = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _scalingUp = false;
    }
}
