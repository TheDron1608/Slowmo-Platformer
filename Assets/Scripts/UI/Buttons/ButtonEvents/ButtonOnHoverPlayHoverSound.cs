using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverPlayHoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AbstractSoundPlayer SoundOnHover;
    [SerializeField]
    private Button _button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.IsInteractable())
        {
            SoundOnHover.PlaySound();
        }
    }
}
