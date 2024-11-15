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

        public ButtonOptionsOption(string title) 
        { 
            Title = title;
        }
    }



    public List<ButtonOptionsOption> Options = new List<ButtonOptionsOption>();
    [SerializeField]
    private TextMeshProUGUI _buttonText;

    private int _currentOptionIndex = 0;

    public event EventHandler<int> ButtonOptions_OnOptionChanged;

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

    public void SetOptionIndex(int newIndex)
    {
        if (newIndex < 0 || newIndex >= Options.Count) throw new IndexOutOfRangeException("trying set _currentOptionIndexout of index");
        _currentOptionIndex = newIndex;
        UpdateCurrentDisplayedOption();
    }

    public void NextOption()
    {
        _currentOptionIndex++;
        if (_currentOptionIndex >= Options.Count)
        {
            _currentOptionIndex = 0;
        }

        UpdateCurrentDisplayedOption();

        ButtonOptions_OnOptionChanged?.Invoke(this, _currentOptionIndex);
    }

    public void PrevOption()
    {
        _currentOptionIndex--;
        if (_currentOptionIndex < 0)
        {
            _currentOptionIndex = Options.Count - 1;
        }

        UpdateCurrentDisplayedOption();

        ButtonOptions_OnOptionChanged?.Invoke(this, _currentOptionIndex);
    }
}
