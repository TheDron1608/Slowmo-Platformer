using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetCharacterButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    const char HIDE_INFO_REPLACE_CHAR = '?';

    public PlayerCharacterInfo PlayerInfo;
    public int FloorLevel = 0;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _button;
    [SerializeField] private Sprite _lockedIconSprite;
    [SerializeField] private CharacterSelectContainer _characterSelectContainer;

    private void Awake()
    {
        SessionManager.Instance.CurrentSessionChanged += Instance_CurrentSessionChanged;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _characterSelectContainer.SetCharacterInfo(
            _button.interactable ?
                PlayerInfo.LocalizedName.GetLocalizedString() :
                PlayerInfo.LocalizedName.GetLocalizedString().FilterReplace(HIDE_INFO_REPLACE_CHAR, false, false, true, true, true, true),
            _button.interactable ? 
                PlayerInfo.LocalizedDesc.GetLocalizedString() :
                PlayerInfo.LocalizedDesc.GetLocalizedString().FilterReplace(HIDE_INFO_REPLACE_CHAR, false, false, true, true, true, true),
            FloorLevel,
            PlayerInfo.InfoBgMaterial,
            PlayerInfo.InfoTextColor
            );
        _characterSelectContainer.SetCharacterInfoVisibility(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _characterSelectContainer.SetCharacterInfoVisibility(false);
    }

    public void SelectCharacter()
    {
        SpawnManager.Instance.PlayerCharacter = PlayerInfo.PlayerCharacter;
        foreach (AbstractModificator mod in PlayerInfo.StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(mod, AbstractModificator.ModificatorStatuses.CHARACTER_DEFAULT);
        }
    }

    private void Instance_CurrentSessionChanged(object sender, System.EventArgs e)
    {
        if (
            SessionManager.Instance == null ||
            SessionManager.Instance.CurrentSession.UnlockedCharacters.Contains(PlayerInfo.GetUnlockCharacterJSONName()) ||
            SessionManager.Instance.DefaultUnlockedCharacters.Contains(PlayerInfo)
            )
        {
            _button.interactable = true;
            _iconImage.sprite = PlayerInfo.CharacterIconSprite;
        }
        else
        {
            _button.interactable = false;
            _iconImage.sprite = _lockedIconSprite;
        }
    }

    private void OnDestroy()
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.CurrentSessionChanged -= Instance_CurrentSessionChanged;
        }
    }
}
