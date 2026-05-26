using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialKeyRoll : MonoBehaviour
{
    public InputActionReference Key;
    public InputActionReference KeyMoveAlt;
    public int KeyboardKeyIdx = 0;
    public int KeyboardMoveDownKeyIdx = 3;
    public int KeyboardMoveLeftKeyIdx = 2;
    public int KeyboardMoveRightKeyIdx = 4;
    public int GamepadKeyIdx = 1;
    public int GamepadMoveKeyIdx = 5;

    private void Awake()
    {
        GetComponent<TextMeshProUGUI>().text =
            CurrentDeviceTracker.GetGamepadIsConnected() ?
            $"[{GetBindingName(Key, GamepadKeyIdx)}] / [{GetBindingName(KeyMoveAlt, GamepadMoveKeyIdx)}]" :
            $"[{GetBindingName(Key, KeyboardKeyIdx)}] / [{GetBindingName(KeyMoveAlt, KeyboardMoveDownKeyIdx)}+{GetBindingName(KeyMoveAlt, KeyboardMoveLeftKeyIdx)}] / [{GetBindingName(KeyMoveAlt, KeyboardMoveDownKeyIdx)}+{GetBindingName(KeyMoveAlt, KeyboardMoveRightKeyIdx)}]";
    }

    private string GetBindingName(InputActionReference actionRef, int idx)
    {
        return 
            actionRef.action.GetBindingDisplayString(
            idx, 
            InputBinding.DisplayStringOptions.DontIncludeInteractions
            );
    }
}