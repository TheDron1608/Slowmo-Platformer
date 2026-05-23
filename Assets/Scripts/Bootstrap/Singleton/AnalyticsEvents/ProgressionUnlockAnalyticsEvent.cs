
public class ProgressionUnlockAnalyticsEvent : Unity.Services.Analytics.Event
{
    public string UnlockName { set { SetParameter("UnclockName", value); } }

    public float SaveTotalTimeSeconds { set { SetParameter("SaveTotalTime", value); } }

    public ProgressionUnlockAnalyticsEvent(string unlockName) : base("ProgressionUnlocks")
    {
        UnlockName = unlockName;

        SaveTotalTimeSeconds = SessionManager.Instance.CurrentSession.TotalPlayTime;
    }
}