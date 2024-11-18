using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BindedKey : MonoBehaviour
{
    public InputActionReference ActionReference;
    public int ActionReferenceIndex = 0;
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
        UpdateKeyButtonText();
    }

    private void InputSystem_OnActionChange(object arg1, InputActionChange change)
    {
        UpdateKeyButtonText();
    }
    private void UpdateKeyButtonText()
    {
        _textContainer.text = GetCurrentInputBindingName();
    }

    public string GetCurrentInputBindingName()
    {
        return ActionReference.action.GetBindingDisplayString(ActionReferenceIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }


    private void OnDestroy()
    {
        InputSystem.onActionChange -= InputSystem_OnActionChange;
    }
}
