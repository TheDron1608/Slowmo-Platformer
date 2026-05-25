using System.Collections;
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

    public InputActionReference RestartAction;
    public InputActionReference LeaveAction;

    [SerializeField] private RectTransform _allDeadTitleContainer;
    [SerializeField] private RectTransform _finishedGameTitleContainer;

    private GameOverReasons _gameOverReason = GameOverReasons.UNSET;
    private Coroutine _restartCoroutine = null;

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
        RestartAction.action.started += RestartActionReference_started;

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
        RestartAction.action.started -= RestartActionReference_started;
    }

    private void RestartActionReference_started(InputAction.CallbackContext obj)
    {
        if (_restartCoroutine == null) _restartCoroutine = StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        if (GameplayUIManager.GetInstance() != null) GameplayUIManager.GetInstance().Pause.Paused = false;

        SessionManager.Instance.ApplyTempSessionToCurrentSessionAndSave();

        yield return SessionManager.Instance.ResetTempSession();
        SessionManager.Instance.InitSelectedPlayer();

        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    private void PauseActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (_restartCoroutine == null) Exit();
    }

    private void Exit()
    {
        if (GameplayUIManager.GetInstance() != null) GameplayUIManager.GetInstance().Pause.Paused = false;

        SessionManager.Instance.ApplyTempSessionToCurrentSessionAndSave();
        SessionManager.Instance.ResetTempSessionData();

        UIManager.Instance.LoadSceneWithEffect(SceneList.MAIN_MENU);
    }
}