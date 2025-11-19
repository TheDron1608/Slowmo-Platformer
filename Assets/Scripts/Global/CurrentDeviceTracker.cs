using UnityEngine;
public static class CurrentDeviceTracker
{
    const int KEYBOARD_BIND_INDEX = 0;
    const int GAMEPAD_BIND_INDEX = 1;

    public static bool GetGamepadIsConnected()
    {
        string[] joystickNames = Input.GetJoystickNames();
        for (int i = 0; i < joystickNames.Length; i++)
        {
            if (joystickNames[i] != "") return true;
        }
        return false;
    }

    public static int GetCurrentDeviceKeyBindIndex()
    {
        return GetGamepadIsConnected() ? GAMEPAD_BIND_INDEX : KEYBOARD_BIND_INDEX;
    }
}
