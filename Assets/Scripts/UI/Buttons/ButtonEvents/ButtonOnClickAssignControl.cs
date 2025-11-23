using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonOnClickAssignControl : MonoBehaviour
{
    const float DELAY_SECONDS_AFTER_REBIND_SECONDS = 0.25f;

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

        GameObject lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(null);

        UIManager.Instance.InputBindingScreenOverlay.Show();

        ActionReference.action.PerformInteractiveRebinding(ActionReferenceIndex).OnComplete(
            callback =>
            {
                ActionReference.action.actionMap.Enable();

                for (int i = 0; i < InputSystem.actions.actionMaps.Count; i++)
                {
                    InputSystem.actions.actionMaps[i].Enable();
                }

                UIManager.Instance.InputBindingScreenOverlay.Hide();
                string newActionData = InputSystem.actions.SaveBindingOverridesAsJson();
                JSONFileManager.SaveJSON(JSONFileManager.Instance.ControlsFileName, newActionData);

                StartCoroutine(SelectGameObjectAfterDuration(lastSelectedGameObject));
            }
            ).Start();
    }

    private IEnumerator SelectGameObjectAfterDuration(GameObject selectObject)
    {
        yield return new WaitForSeconds(DELAY_SECONDS_AFTER_REBIND_SECONDS);

        EventSystem.current.SetSelectedGameObject(selectObject);
    }
}
