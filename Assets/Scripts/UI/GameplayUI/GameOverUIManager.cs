using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUIManager : MonoBehaviour
{
    public enum GameOverReasons
    {
        UNSET,
        ALL_DEAD,
        FINISHED_GAME
    }

    public InputActionReference LeaveAction;
    public string MainMenuSceneName;

    [SerializeField] private RectTransform _allDeadTitleContainer;
    [SerializeField] private RectTransform _finishedGameTitleContainer;

    private GameOverReasons _gameOverReason = GameOverReasons.UNSET;

    public GameOverReasons GameOverReason
    {
        get => _gameOverReason;
        set
        {
            if (value == _gameOverReason) return;

            _gameOverReason = value;
            UpdateGameOverTitle();
        }
    }

    private void Start()
    {
        LeaveAction.action.started += PauseActionReference_OnActionStarted;

        UpdateGameOverTitle();
    }

    private void UpdateGameOverTitle()
    {
        _allDeadTitleContainer.gameObject.SetActive(false);
        _finishedGameTitleContainer.gameObject.SetActive(false);

        switch (GameOverReason)
        {
            case GameOverReasons.ALL_DEAD:
                _allDeadTitleContainer.gameObject.SetActive(true);
                break;
            case GameOverReasons.FINISHED_GAME:
                _finishedGameTitleContainer.gameObject.SetActive(true);
                break;
        }
    }

    private void OnDestroy()
    {
        LeaveAction.action.started -= PauseActionReference_OnActionStarted;
    }

    private void PauseActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (GameplayUIManager.GetInstance() != null) GameplayUIManager.GetInstance().Pause.Paused = false;

        SessionManager.Instance.ApplyTempSessionToCurrentSessionAndSave();
        SessionManager.Instance.CurrentSession = null;

        UIManager.Instance.LoadSceneWithEffect(MainMenuSceneName);
    }
}