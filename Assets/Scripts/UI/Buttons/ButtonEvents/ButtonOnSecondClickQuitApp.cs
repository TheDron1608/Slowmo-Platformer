using System.Collections;
using TMPro;
using UnityEngine;

public class ButtonOnSecondClickQuitApp : MonoBehaviour
{
    const float RESET_WARNING_DELAY_SECONDS = 5f;

    [SerializeField]
    private TextMeshProUGUI _quitText;
    [SerializeField]
    private TextMeshProUGUI _warnText;

    private bool warned = false;
    private Coroutine resetWarnCoroutine;

    public void AttemptQuit()
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
            Application.Quit();
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
