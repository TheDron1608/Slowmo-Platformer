[AnalyticsEventName("ProgressionUnlocks")]
public class ProgressionUnlockAnalyticsEvent : AbstractAnalyticsEvent
{
    [AnalyticsPropName("UnclockName")]
    public string UnlockName;

    [AnalyticsPropName("SaveTotalTime")]
    public float SaveTotalTimeSeconds;

    public ProgressionUnlockAnalyticsEvent(string unlockName)
    {
        UnlockName = unlockName;

        SaveTotalTimeSeconds = SessionManager.Instance.CurrentSession.TotalPlayTime;
    }
}