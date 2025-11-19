using TMPro;
using UnityEngine;

public class GameOverSurvivedScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    private string _localizedText = "Survived: ";

    public void UpdateLocalizedText(string value)
    {
        _localizedText = value;
        _text.text = _localizedText + SessionManager.Instance.TempSession.CurrentPlayTime;
    }
}