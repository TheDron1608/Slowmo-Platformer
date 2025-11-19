using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUIManager : MonoBehaviour
{
    public InputActionReference LeaveAction;
    public string MainMenuSceneName;

    public static GameOverUIManager GetInstance()
    {
        return UIManager.Instance.GameOverScreenOverlay.GetGameOverUI();
    }

    private void Start()
    {
        LeaveAction.action.started += PauseActionReference_OnActionStarted;
    }

    private void OnDestroy()
    {
        LeaveAction.action.started -= PauseActionReference_OnActionStarted;
    }

    private void PauseActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (GameplayUIManager.GetInstance() != null) GameplayUIManager.GetInstance().Pause.Paused = false;
        SessionManager.Instance.CurrentSession = null;

        UIManager.Instance.LoadSceneWithEffect(MainMenuSceneName);
    }
}