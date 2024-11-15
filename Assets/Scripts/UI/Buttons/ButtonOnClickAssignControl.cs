using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonOnClickAssignControl : MonoBehaviour
{
    public InputActionReference ActionReference;
    public int ActionReferenceIndex = 0;
    [SerializeField] private BindedKey _BindedKey;

    private void Awake()
    {
        _BindedKey.ActionReference = ActionReference;
        _BindedKey.ActionReferenceIndex = ActionReferenceIndex;
    }

    public void AssignButton(InputAction newAction)
    {
        ActionReference.Set(newAction);
    }
}
