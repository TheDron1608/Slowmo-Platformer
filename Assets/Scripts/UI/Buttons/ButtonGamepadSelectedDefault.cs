using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonGamepadSelectedDefault : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateReselect();
        InputSystem.onDeviceChange += inputSystem_OnDevicesChange;
    }

    private void inputSystem_OnDevicesChange(InputDevice device, InputDeviceChange change)
    {
        UpdateReselect();
    }

    private void UpdateReselect()
    {
        if (EventSystem.current.currentSelectedGameObject == null && GetGamepadIsConnected())
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    private bool GetGamepadIsConnected()
    {
        string[] joystickNames = Input.GetJoystickNames();
        for (int i = 0; i < joystickNames.Length; i++) 
        {
            if (joystickNames[i] != "") return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= inputSystem_OnDevicesChange;
    }
}
