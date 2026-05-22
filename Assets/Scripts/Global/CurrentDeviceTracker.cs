using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public static class CurrentDeviceTracker
{
    const int KEYBOARD_BIND_INDEX = 0;
    const int GAMEPAD_BIND_INDEX = 1;

    public static bool GetGamepadIsConnected()
    {
        return InputSystem.devices.Any(e => e is Joystick || e is Gamepad);
    }

    public static int GetCurrentDeviceKeyBindIndex()
    {
        return GetGamepadIsConnected() ? GAMEPAD_BIND_INDEX : KEYBOARD_BIND_INDEX;
    }

    public static Vector3? GetMouseWorldPositionOnLayer(ZIndexLayer layer)
    {
        if (Mouse.current == null) return null;

        RaycastHit[] mouseHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        for (int i = 0; i < mouseHits.Length; i++)
        {
            if (mouseHits[i].collider.gameObject == layer.gameObject)
            {
                return mouseHits[i].point;
            }
        }
        return null;
    }
}
