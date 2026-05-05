using TMPro;
using UnityEngine;
using UnityEngine.Localization;
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
    [SerializeField] private RectTransform _customDescContainer;
    [SerializeField] private LocalizedString _localizedLockedTitle;

    private PlayerCharacterInfo _currentInfo = null;

    public void SetCharacterInfo(PlayerCharacterInfo info, int floorLevel)
    {
        if (_currentInfo == info) return;

        _cityAnimator.CurrentLevel = floorLevel;

        _background.material = info.InfoBgMaterial;
        _characterName.color = info.InfoTextColor;
        _characterDesc.color = info.InfoTextColor;

        foreach (Transform child in _customDescContainer)
        {
            Destroy(child.gameObject);
        }

        if (SessionManager.Instance?.GetCharacterIsUnlocked(info) ?? true)
        {
            _characterName.text = info.LocalizedName.GetLocalizedString();
            _characterDesc.text = info.LocalizedDesc.GetLocalizedString();
            _characterDesc.enabled = true;
        }
        else
        {
            if (info.CustomUnlockConditionInfo != null)
            {
                _characterDesc.enabled = false;
                Instantiate(info.CustomUnlockConditionInfo, _customDescContainer);
            }
            else
            {
                _characterName.text = _localizedLockedTitle.GetLocalizedString();
                _characterDesc.text = info.LocalizedUnlockCondition.GetLocalizedString();
                _characterDesc.enabled = true;
            }
        }

        _currentInfo = info;
    }

    public void SetCharacterInfoVisibility(bool value)
    {
        _characterInfoMove.StartMoving(value ? _characterInfoShownTargetPosition : _characterInfoHiddenTargetPosition);
    }
}
