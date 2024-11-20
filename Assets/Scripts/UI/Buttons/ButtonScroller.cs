using System;
using TMPro;
using UnityEngine;

public class ButtonScroller : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _buttonText;

    [SerializeField] 
    private int _currentValue;

    public int CurrentValue
    {
        get
        {
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            OnScrollChanged?.Invoke(this, _currentValue);
            UpdateButtonText();
        }
    }

    public int MinValue = 0;
    public int MaxValue = 10;
    public bool ReverseOnOverflow = false;

    public event EventHandler<int> OnScrollChanged;



    private void Start()
    {
        UpdateButtonText();
    }

    public void NextValue()
    {
        _currentValue++;
        if (_currentValue > MaxValue)
        {
            if (ReverseOnOverflow)
            {
                _currentValue = MinValue;
            }
            else
            {
                _currentValue = MaxValue;
            }
        }
        OnScrollChanged?.Invoke(this, _currentValue);
        UpdateButtonText();
    }

    public void PrevValue()
    {
        _currentValue--;
        if (_currentValue < MinValue)
        {
            if (ReverseOnOverflow)
            {
                _currentValue = MaxValue;
            }
            else
            {
                _currentValue = MinValue;
            }
        }
        OnScrollChanged?.Invoke(this, _currentValue);
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        _buttonText.text = _currentValue.ToString();
    }
}
