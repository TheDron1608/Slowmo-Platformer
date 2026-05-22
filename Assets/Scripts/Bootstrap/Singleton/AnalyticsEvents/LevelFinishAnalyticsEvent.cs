using System.Linq;

[AnalyticsEventName("LevelFinishedStats")]
public class LevelFinishAnalyticsEvent : AbstractAnalyticsEvent
{
    [AnalyticsPropName("PlayerTotalPlaytimeSeconds")]
    public float PlayerTotalPlayTime;

    [AnalyticsPropName("PlayerCharacter")]
    public string PlayerCharacterName;

    [AnalyticsPropName("TotalModPrice")]
    public float TotalModsPrice;

    [AnalyticsPropName("AveragePlayerHealth")]
    public float AvgPlayerHealth;

    [AnalyticsPropName("MaxPlayerHealth")]
    public float MaxPlayerHealth;

    [AnalyticsPropName("MinPlayerHealth")]
    public float MinPlayerHealth;

    [AnalyticsPropName("AverageCombo")]
    public float AvgCombo;

    [AnalyticsPropName("MaxCombo")]
    public int MaxCombo;

    [AnalyticsPropName("GameSessionTimeSeconds")]
    public float GameSessionTime;

    public LevelFinishAnalyticsEvent()
    {
        PlayerTotalPlayTime = SessionManager.Instance.Sessions.Sum(e => e.TotalPlayTime);

        PlayerCharacterName = SpawnManager.Instance.PlayerCharacter.gameObject.name;

        TotalModsPrice = ModificatorsManager.Instance.GetTotalModsPrice();

        AvgPlayerHealth = AnalyticsManager.Instance.TrackedPlayerHealth.Average();
        MaxPlayerHealth = AnalyticsManager.Instance.TrackedPlayerHealth.Max();
        MinPlayerHealth = AnalyticsManager.Instance.TrackedPlayerHealth.Min();

        AvgCombo = (float)AnalyticsManager.Instance.TrackedCombo.Average();
        MaxCombo = AnalyticsManager.Instance.TrackedCombo.Max();

        GameSessionTime = DifficultyManager.Instance.RealtimeTotalDifficultyTime;
    }
}