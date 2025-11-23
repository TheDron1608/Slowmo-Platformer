using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class GameOverExit : MonoBehaviour
{
    public InputActionReference ExitAction;
    [SerializeField] private LocalizeStringEvent _localizedText;
    [SerializeField] private GameOverUIManager _gameOverUI;

    private void Awake()
    {
        UpdateText();
        InputSystem.onDeviceChange += InputSystem_OnDeviceChange;
    }

    private void InputSystem_OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        UpdateText();
    }

    public void UpdateText()
    {
        (_localizedText.StringReference["ExitButton"] as StringVariable).Value =
            _gameOverUI.LeaveAction.action.GetBindingDisplayString(CurrentDeviceTracker.GetCurrentDeviceKeyBindIndex()).ToUpper();
    }
}