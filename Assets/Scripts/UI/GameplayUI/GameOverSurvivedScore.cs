using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class GameOverSurvivedScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    private string _localizedText = "Survived: ";

    public void UpdateLocalizedText(string value)
    {
        _localizedText = value;
        TimeSpan surviveTime = new(0, 0, (int)math.round(DifficultyManager.Instance.TotalDifficultyTime));
        _text.text = _localizedText + surviveTime.ToString(@"mm\:ss");
    }
}