using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonOnClickAssignControl : MonoBehaviour
{
    public InputActionReference ActionReference;
    public int ActionReferenceIndex = 0;
    [SerializeField] private BindedKey _bindedKey;

    private void Awake()
    {
        _bindedKey.ActionReference = ActionReference;
        _bindedKey.ActionReferenceIndexKeyboard = ActionReferenceIndex;
    }

    public void AssignButton()
    {
        ActionReference.action.actionMap.Disable();
        UIManager.Instance.InputBindingScreenOverlay.Show();
        ActionReference.action.PerformInteractiveRebinding(ActionReferenceIndex).OnComplete(
            callback => { 

                ActionReference.action.actionMap.Enable(); 
                UIManager.Instance.InputBindingScreenOverlay.Hide();
                string newActionData = InputSystem.actions.SaveBindingOverridesAsJson();
                JSONFileManager.SaveJSON(JSONFileManager.Instance.ControlsFileName, newActionData);

            }
            ).Start();
    }
}
