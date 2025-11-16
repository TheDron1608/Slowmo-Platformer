using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class CharacterSelectContainer : MonoBehaviour
{
    [SerializeField] private CityAnimatorParameters _cityAnimator;
    [SerializeField] private TextMeshProUGUI _characterName;
    [SerializeField] private TextMeshProUGUI _characterDesc;
    [SerializeField] private Image _background; 
    [SerializeField] private MoveBetweenTwoCoors _characterInfoMove;
    [SerializeField] private GameObject _characterInfoShownTargetPosition;
    [SerializeField] private GameObject _characterInfoHiddenTargetPosition;

    public void SetCharacterInfo(string characterName, string characterDesc, int floorLevel, Material backgroundMaterial, Color textColor)
    {
        _characterName.text = characterName;
        _characterDesc.text = characterDesc;
        _cityAnimator.CurrentLevel = floorLevel;

        _background.material = backgroundMaterial;  
        _characterName.color = textColor;
        _characterDesc.color = textColor;

    }

    public void SetCharacterInfoVisibility(bool value)
    {
        _characterInfoMove.StartMoving(value ? _characterInfoShownTargetPosition : _characterInfoHiddenTargetPosition);
    }
}
