using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonOnSecondClickQuitGameplay : MonoBehaviour
{
    const float RESET_WARNING_DELAY_SECONDS = 5f;

    public string MainMenuScenename;

    [SerializeField]
    private TextMeshProUGUI _quitText;
    [SerializeField]
    private TextMeshProUGUI _warnText;

    private bool warned = false;
    private Coroutine resetWarnCoroutine;

    public void AttemptChangeScene()
    {
        if (!warned)
        {
            _quitText.gameObject.SetActive(false);
            _warnText.gameObject.SetActive(true);
            warned = true;
            resetWarnCoroutine = StartCoroutine(ResetWarningAfterDelay());
        }
        else
        {
            StopCoroutine(resetWarnCoroutine);
            GameplayUIManager.GetInstance().Pause.Paused = false;
            UIManager.Instance.LoadSceneWithEffect(MainMenuScenename);
        }
    }

    private IEnumerator ResetWarningAfterDelay()
    {
        yield return new WaitForSeconds(RESET_WARNING_DELAY_SECONDS);

        _quitText.gameObject.SetActive(true);
        _warnText.gameObject.SetActive(false);
        warned = false;
    }
}
