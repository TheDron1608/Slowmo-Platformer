using System.Collections;
using TMPro;
using UnityEngine;

public class ButtonOnSecondClickQuitGameplay : MonoBehaviour
{
    const float RESET_WARNING_DELAY_SECONDS = 5f;

    public string MainMenuScenename;

    [SerializeField]
    private TextMeshProUGUI _quitText;
    [SerializeField]
    private TextMeshProUGUI _warnText;

    private bool _warned = false;
    private Coroutine _resetWarnCoroutine = null;
    private Coroutine _exitCoroutine = null;

    public void QuiatGameplay()
    {
        if (!_warned)
        {
            _quitText.gameObject.SetActive(false);
            _warnText.gameObject.SetActive(true);
            _warned = true;
            _resetWarnCoroutine = StartCoroutine(ResetWarningAfterDelay());
        }
        else
        {
            StopCoroutine(_resetWarnCoroutine);

            if (_exitCoroutine == null) _exitCoroutine = StartCoroutine(ExitCoroutine());
        }
    }

    private IEnumerator ResetWarningAfterDelay()
    {
        yield return new WaitForSeconds(RESET_WARNING_DELAY_SECONDS);

        _quitText.gameObject.SetActive(true);
        _warnText.gameObject.SetActive(false);
        _warned = false;
    }

    private IEnumerator ExitCoroutine()
    {
        SessionManager.Instance.ApplyTempSessionToCurrentSessionAndSave();
        yield return SessionManager.Instance.ResetTempSession();

        UIManager.Instance.LoadSceneWithEffect(MainMenuScenename);
    }
}
