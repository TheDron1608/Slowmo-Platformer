using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverExit : MonoBehaviour
{
    const string INSERT_EXIT_BUTTON_VAR = "[EXIT_BUTTON]";
    const int KEYBOARD_BIND_INDEX = 0;
    const int GAMEPAD_BIND_INDEX = 1;

    public InputActionReference ExitAction;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameOverUIManager _gameOverUI;

    private string _localizedText = "Press [EXIT_BUTTON] to exit";

    private void Awake()
    {
        InputSystem.onDeviceChange += InputSystem_OnDeviceChange;
    }

    private void InputSystem_OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        UpdateText();
    }

    public void UpdateLocalizedText(string value)
    {
        _localizedText = value;
        UpdateText();
    }

    private void UpdateText()
    {
        _text.text = _localizedText.Replace(
            INSERT_EXIT_BUTTON_VAR,
            _gameOverUI.LeaveAction.action.GetBindingDisplayString(
                CurrentDeviceTracker.GetGamepadIsConnected() ? GAMEPAD_BIND_INDEX : KEYBOARD_BIND_INDEX,
                InputBinding.DisplayStringOptions.DontIncludeInteractions
                ).ToUpper()
            );
    }
}