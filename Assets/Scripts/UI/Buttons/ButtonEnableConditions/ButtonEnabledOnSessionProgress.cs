using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEnabledOnSessionProgress : MonoBehaviour
{
    public bool ChangeTextOnly = false;

    [SerializeField]
    private int _requiredFloorProgress = -1;
    [SerializeField]
    private int _requiredLevelProgress = -1;


    [SerializeField]
    private Button _button;
    [SerializeField]
    private TextMeshProUGUI _textContainer;
    [SerializeField]
    private string _disabledText;

    private string _baseText;


    public int RequiredFloorProgress
    {
        get => _requiredFloorProgress;
        set
        {
            _requiredFloorProgress = value;
            UpdateEnabled();
        }
    }
    public int RequiredLevelProgress
    {
        get => _requiredLevelProgress;
        set
        {
            _requiredLevelProgress = value;
            UpdateEnabled();
        }
    }

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
        bool state =
            SessionManager.Instance.CurrentSession != null &&
            (
                SessionManager.Instance.CurrentSession.FloorProgress > _requiredFloorProgress || 
                (SessionManager.Instance.CurrentSession.LevelProgress >= _requiredLevelProgress && SessionManager.Instance.CurrentSession.FloorProgress >= _requiredFloorProgress)
            );

        if (!ChangeTextOnly)
        {
            _button.interactable = state;
        }
        if (_textContainer != null)
        {
            _textContainer.text = state ? _baseText : _disabledText;
        }
    }

    private void OnDestroy()
    {
        if (SessionManager.Instance == null) return;
        SessionManager.Instance.CurrentSessionChanged -= SessionManager_OnCurrentSessionChanged;
    }
}
