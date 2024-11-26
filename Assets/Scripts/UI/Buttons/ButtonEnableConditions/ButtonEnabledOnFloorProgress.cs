using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEnabledOnFloorProgress : MonoBehaviour
{
    [SerializeField]
    private int _requiredProgress;
    [SerializeField]
    private Button _button;
    [SerializeField]
    private TextMeshProUGUI _textContainer;
    [SerializeField]
    private string _disabledText;

    private string _baseText;


    private void Start()
    {
        SessionManager.Instance.CurrentSessionChanged += SessionManager_OnCurrentSessionChanged;

        if (_textContainer != null )
        {
            _baseText = _textContainer.text;
        }

        UpdateEnabled();
    }

    private void SessionManager_OnCurrentSessionChanged(object sender, EventArgs e)
    {
        UpdateEnabled();
    }

    private void UpdateEnabled()
    {
        bool state = SessionManager.Instance.CurrentSession != null && SessionManager.Instance.CurrentSession.FloorProgress >= _requiredProgress;
        _button.interactable = state;
        if (_textContainer != null)
        {
            _textContainer.text = state ? _baseText : _disabledText;
        }
    }
}
