using UnityEngine;
public static class CurrentDeviceTracker
{
    public static bool GetGamepadIsConnected()
    {
        string[] joystickNames = Input.GetJoystickNames();
        for (int i = 0; i < joystickNames.Length; i++)
        {
            if (joystickNames[i] != "") return true;
        }
        return false;
    }
}
