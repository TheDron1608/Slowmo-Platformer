using TMPro;
using UnityEngine;

public class GameOverKilledScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    private string _localizedText = "Killed: ";

    public void UpdateLocalizedText(string value)
    {
        _localizedText = value;
        _text.text = _localizedText + SessionManager.Instance.CurrentSession.CurrentKills;
    }
}