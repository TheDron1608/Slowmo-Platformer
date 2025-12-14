using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonOnHoverSetAsLastSibling : MonoBehaviour, IPointerEnterHandler
{
    public Transform SetWho;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetWho.SetAsLastSibling();
    }
}
