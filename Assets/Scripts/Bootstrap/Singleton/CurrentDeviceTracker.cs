using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class CurrentDeviceTracker : MonoBehaviour
{
    public static CurrentDeviceTracker Instance { get; private set; }

    


    private void Awake()
    {
        DontDestroyOnLoad(this);

        InputSystem.onDeviceChange += InputSystem_OnDeviceChange;

        if (Instance != null) throw new UnityException("Limit of 1 Instance of JSONFileManager objects");
        Instance = this;
    }

    private void InputSystem_OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        //Debug.Log(change.HumanName());
    }



    private void OnDestroy()
    {
        Instance = null;
    }
}
