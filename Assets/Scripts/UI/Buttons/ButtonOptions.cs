using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonOptions : MonoBehaviour
{
    [Serializable]
    public class ButtonOptionsOption
    {
        public string Title;
    }

    public List<ButtonOptionsOption> Options = new List<ButtonOptionsOption>();

    [SerializeField]
    private TextMeshProUGUI _buttonText;

    private int _currentOptionIndex = 0;

    private void Start()
    {
        UpdateCurrentDisplayedOption();
    }

    private void UpdateCurrentDisplayedOption()
    {
        if (Options.Count > 0)
        {
            _buttonText.text = Options[_currentOptionIndex].Title;
        }
        else
        {
            _buttonText.text = "EMPTY";
        }
    }

    public ButtonOptionsOption GetCurrentOption()
    {
        return Options[_currentOptionIndex];
    }

    public void NextOption()
    {
        _currentOptionIndex++;
        if (_currentOptionIndex >= Options.Count)
        {
            _currentOptionIndex = 0;
        }
        UpdateCurrentDisplayedOption();
    }

    public void PrevOption()
    {
        _currentOptionIndex--;
        if (_currentOptionIndex < 0)
        {
            _currentOptionIndex = Options.Count - 1;
        }
        UpdateCurrentDisplayedOption();
    }
}
