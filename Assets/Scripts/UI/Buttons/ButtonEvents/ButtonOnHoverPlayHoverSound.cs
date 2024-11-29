using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverPlayHoverSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private Button _button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.IsInteractable())
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.ButtonSelectSound);
        }
    }
}
