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

    public int CurrentOptionIndex { get; private set; } = 0;

    public event EventHandler<int> OnOptionChanged;

    private void Start()
    {
        UpdateCurrentDisplayedOption();
    }

    private void UpdateCurrentDisplayedOption()
    {
        if (Options.Count > 0)
        {
            _buttonText.text = Options[CurrentOptionIndex].Title;
        }
        else
        {
            _buttonText.text = "EMPTY";
        }
    }

    public ButtonOptionsOption GetCurrentOption()
    {
        return Options[CurrentOptionIndex];
    }

    public void SetOptionIndex(int newIndex)
    {
        if (newIndex < 0 || newIndex >= Options.Count) throw new IndexOutOfRangeException("trying set _currentOptionIndexout out of max index");
        CurrentOptionIndex = newIndex;
        OnOptionChanged?.Invoke(this, CurrentOptionIndex);
        UpdateCurrentDisplayedOption();
    }

    public void NextOption()
    {
        CurrentOptionIndex++;
        if (CurrentOptionIndex >= Options.Count)
        {
            CurrentOptionIndex = 0;
        }

        UpdateCurrentDisplayedOption();

        OnOptionChanged?.Invoke(this, CurrentOptionIndex);
    }

    public void PrevOption()
    {
        CurrentOptionIndex--;
        if (CurrentOptionIndex < 0)
        {
            CurrentOptionIndex = Options.Count - 1;
        }

        UpdateCurrentDisplayedOption();

        OnOptionChanged?.Invoke(this, CurrentOptionIndex);
    }
}
