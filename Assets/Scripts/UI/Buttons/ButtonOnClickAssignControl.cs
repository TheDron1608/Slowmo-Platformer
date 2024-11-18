using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonOnClickAssignControl : MonoBehaviour
{
    public InputActionReference ActionReference;
    public int ActionReferenceIndex = 0;
    [SerializeField] private BindedKey _bindedKey;

    private string _oldBindedKeyText;

    private void Awake()
    {
        _bindedKey.ActionReference = ActionReference;
        _bindedKey.ActionReferenceIndex = ActionReferenceIndex;
    }

    public void AssignButton()
    {
        ActionReference.action.actionMap.Disable();
        UIManager.Instance.InputBindingScreenOverlay.Show();
        ActionReference.action.PerformInteractiveRebinding(ActionReferenceIndex).OnComplete(
            callback => { 
                ActionReference.action.actionMap.Enable(); 
                _oldBindedKeyText = null;
                UIManager.Instance.InputBindingScreenOverlay.Hide();
            }
            ).Start();
    }
}
