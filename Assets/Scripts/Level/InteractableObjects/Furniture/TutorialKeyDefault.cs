using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialKeyDefault : MonoBehaviour
{
    public InputActionReference Key;
    public int KeyboardKeyIdx = 0;
    public int GamepadKeyIdx = 1;

    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text = $"[{GetCurrentBindingName()}]";
    }

    private string GetCurrentBindingName()
    {
        return 
            Key.action.GetBindingDisplayString(
            CurrentDeviceTracker.GetGamepadIsConnected() ? GamepadKeyIdx : KeyboardKeyIdx, 
            InputBinding.DisplayStringOptions.DontIncludeInteractions
            );
    }
}