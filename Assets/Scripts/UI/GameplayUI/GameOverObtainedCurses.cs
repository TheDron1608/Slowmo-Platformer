using TMPro;
using UnityEngine;

public class GameOverObtainedCurses : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    private string _localizedText = "Obtained curses: ";

    public void UpdateLocalizedText(string value)
    {
        _localizedText = value;
        _text.text = _localizedText + SessionManager.Instance.CurrentSession.CurrentObtainedCurses;
    }
}