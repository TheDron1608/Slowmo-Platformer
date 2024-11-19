using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class BindedKey : MonoBehaviour
{
    public InputActionReference ActionReference;
    public int ActionReferenceIndexKeyboard = 0;
    public int ActionReferenceIndexGamePad = 1;
    [SerializeField] private TextMeshProUGUI _textContainer;

    public string Text
    {
        get 
        { 
            return _textContainer.text; 
        }
        set 
        { 
            _textContainer.text = value; 
        }
    }


    private void Start()
    {
        InputSystem.onActionChange += InputSystem_OnActionChange;
        InputSystem.onDeviceChange += InputSystem_OnDeviceChange;
        UpdateKeyButtonText();
    }

    private void InputSystem_OnActionChange(object arg1, InputActionChange change)
    {
        if (change != InputActionChange.BoundControlsChanged) return;
        UpdateKeyButtonText();
    }
    private void InputSystem_OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
    {
        UpdateKeyButtonText();
    }

    private void UpdateKeyButtonText()
    {
        _textContainer.text = GetCurrentInputBindingName();
    }

    public string GetCurrentInputBindingName()
    {
        //Debug.Log(InputSystem.devices[^1].device.deviceId);

        return ActionReference.action.GetBindingDisplayString(ActionReferenceIndexKeyboard, InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }


    private void OnDestroy()
    {
        InputSystem.onActionChange -= InputSystem_OnActionChange;
    }
}
