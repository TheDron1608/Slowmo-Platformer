using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUIManager : MonoBehaviour
{
    const float MUSIC_VOLUME_ON_GAME_OVER = 0.1f;

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
    private Coroutine _saveAndQuitCoroutine = null;

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

    private void OnEnable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.TargetMusicVolume = MUSIC_VOLUME_ON_GAME_OVER;
        }
    }

    private void OnDisable()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.TargetMusicVolume = 1f;
        }
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
        if (_saveAndQuitCoroutine == null) _saveAndQuitCoroutine = StartCoroutine(RestartCoroutine());
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
        if (_saveAndQuitCoroutine == null) _saveAndQuitCoroutine = StartCoroutine(Exit());
    }

    private IEnumerator Exit()
    {
        if (GameplayUIManager.GetInstance() != null) GameplayUIManager.GetInstance().Pause.Paused = false;

        SessionManager.Instance.ApplyTempSessionToCurrentSessionAndSave();
        yield return SessionManager.Instance.ResetTempSession();

        UIManager.Instance.LoadSceneWithEffect(SceneList.MAIN_MENU);
    }
}