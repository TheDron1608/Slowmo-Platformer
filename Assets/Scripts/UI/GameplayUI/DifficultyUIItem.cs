using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyUIItem : MonoBehaviour
{
    public Image IconImage;
    public TextMeshProUGUI TitleText;

    private bool _isUsedInTimeline = false;

    public bool IsUsedInTimeline
    {
        get => _isUsedInTimeline;
        set => _isUsedInTimeline = value;
    }
}
