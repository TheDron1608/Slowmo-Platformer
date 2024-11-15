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
        string fullBindPath = ActionReference.action.bindings[ActionReferenceIndex].path;
        return fullBindPath.Substring(fullBindPath.LastIndexOf("/") + 1).FirstCharacterToUpper();
    }


    private void OnDestroy()
    {
        InputSystem.onActionChange -= InputSystem_OnActionChange;
    }
}
