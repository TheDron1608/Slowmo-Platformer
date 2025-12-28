using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ButtonOnHoverSetCharacterInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int FloorLevel = 0;
    [SerializeField] private CharacterSelectContainer _characterSelectContainer;
    [SerializeField] private Material BackgroundMaterial;
    [SerializeField] private Color TextColor;

    private string _localizedCharacterName = "CharacterName";
    private string _localizedCharacterDesc = "CharacterDesc";

    public void UpdateLocalizedCharacterName(string value)
    {
        _localizedCharacterName = value;
    }
    public void UpdateLocalizedCharacterDesc(string value)
    {
        _localizedCharacterDesc = value;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _characterSelectContainer.SetCharacterInfo(_localizedCharacterName, _localizedCharacterDesc, FloorLevel, BackgroundMaterial, TextColor);
        _characterSelectContainer.SetCharacterInfoVisibility(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _characterSelectContainer.SetCharacterInfoVisibility(false);
    }
}
