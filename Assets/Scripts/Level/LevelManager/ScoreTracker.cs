using System;
using System.Collections;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker Instance;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 ScoreTracker per scene");
        Instance = this;
    }

    public void AddKill()
    {
        SessionManager.Instance.TempSession.CurrentKills++;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(PlayTimeSecondsCounter());
    }

    private IEnumerator PlayTimeSecondsCounter()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (!UIManager.GamePaused())
            {
                SessionManager.Instance.TempSession.CurrentPlayTime += new TimeSpan(0, 0, 1);
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}