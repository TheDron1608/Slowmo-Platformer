using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
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
        _characterSelectContainer.SetCharacterInfo(PlayerInfo, FloorLevel);
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
        SpawnManager.Instance.PlayerCharacterHoldable = PlayerInfo.StartHoldable;

        AnalyticsManager.Instance.RecordEvent(new StartGameAnalyticsEvent(PlayerInfo.PlayerCharacter.gameObject.name));
    }

    private void Instance_CurrentSessionChanged(object sender, System.EventArgs e)
    {
        if (SessionManager.Instance?.GetCharacterIsUnlocked(PlayerInfo) ?? true)
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
