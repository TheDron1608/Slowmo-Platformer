using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnInitUpdateSelfLevelInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textContainer;
    [SerializeField]
    private Image _imageCointainer;
    [SerializeField]
    private Sprite _bossLevelSprite;

    private LevelInfo _levelInfo;

    public LevelInfo LevelInfo
    {
        get => _levelInfo;
        set
        {
            _levelInfo = value;
            UpdateLevelInfo();
        }
    }

    private void UpdateLevelInfo()
    {
        _textContainer.text = _levelInfo.LevelName;

        if (LevelInfo.BossLevel)
        {
            _imageCointainer.sprite = _bossLevelSprite;
        }
    }
}
