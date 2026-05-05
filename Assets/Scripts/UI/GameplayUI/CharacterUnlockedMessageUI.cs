using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUnlockedMessageUI : MonoBehaviour
{
    public float MessageDelay;

    public TextMeshProUGUI UnlockedText;
    public TextMeshProUGUI UnlockedWhatText;
    public Image IconImage;
    public Image BgImage;

    [SerializeField] private UIElementTrackTarget _startPosition;
    [SerializeField] private UIElementTrackTarget _endPosition;

    private float _swapFromReachedTime = 0f;

    public void SetUnlockedCharacterInfo(PlayerCharacterInfo unlockedCharacter)
    {
        UnlockedWhatText.text = unlockedCharacter.LocalizedName.GetLocalizedString();
        UnlockedWhatText.color = unlockedCharacter.InfoTextColor;

        UnlockedText.color = unlockedCharacter.InfoTextColor;

        IconImage.sprite = unlockedCharacter.CharacterIconSprite;
        IconImage.material = unlockedCharacter.InfoIconMaterial;

        BgImage.material = unlockedCharacter.InfoBgMaterial;
    }

    private void OnEnable()
    {
        _startPosition.enabled = false;
        _endPosition.enabled = true;
        _swapFromReachedTime = 0f;
    }

    private void Update()
    {
        if (_endPosition.GetIsReachedTrackTarget())
        {
            _swapFromReachedTime += Time.unscaledDeltaTime;
            if (_swapFromReachedTime >= MessageDelay)
            {
                _startPosition.enabled = true;
                _endPosition.enabled = false;
            }
        }

        if (_startPosition.GetIsReachedTrackTarget())
        {
            if (_swapFromReachedTime >= MessageDelay)
            {
                UIManager.Instance.UnlockedCharacterMessageOverlay.Hide();
            }
        }
    }
}