using System.Linq;

public class LevelFinishAnalyticsEvent : Unity.Services.Analytics.Event
{
    public float PlayerTotalPlayTime { set { SetParameter("PlayerTotalPlaytimeSeconds", value); } }

    public string PlayerCharacterName { set { SetParameter("PlayerCharacter", value); } }

    public float TotalModsPrice { set { SetParameter("TotalModPrice", value); } }

    public float AvgPlayerHealth { set { SetParameter("AveragePlayerHealth", value); } }

    public float MaxPlayerHealth { set { SetParameter("MaxPlayerHealth", value); } }

    public float MinPlayerHealth { set { SetParameter("MinPlayerHealth", value); } }

    public float AvgCombo { set { SetParameter("AverageCombo", value); } }

    public int MaxCombo { set { SetParameter("MaxCombo", value); } }

    public float GameSessionTime { set { SetParameter("GameSessionTimeSeconds", value); } }

    public LevelFinishAnalyticsEvent() : base("LevelFinishedStats")
    {
        PlayerTotalPlayTime = SessionManager.Instance.Sessions.Sum(e => e.TotalPlayTime);

        PlayerCharacterName = SpawnManager.Instance.PlayerCharacter.gameObject.name;

        TotalModsPrice = ModificatorsManager.Instance.GetTotalModsPrice();

        if (AnalyticsManager.Instance.TrackedPlayerHealth.Count > 0)
        {
            AvgPlayerHealth = AnalyticsManager.Instance.TrackedPlayerHealth.Average();
            MaxPlayerHealth = AnalyticsManager.Instance.TrackedPlayerHealth.Max();
            MinPlayerHealth = AnalyticsManager.Instance.TrackedPlayerHealth.Min();
        }
        else
        {
            AvgPlayerHealth = 0f;
            MaxPlayerHealth = 0f;
            MinPlayerHealth = 0f;
        }

        AvgCombo = (float)AnalyticsManager.Instance.TrackedCombo.Average();
        MaxCombo = AnalyticsManager.Instance.TrackedCombo.Max();

        GameSessionTime = DifficultyManager.Instance.RealtimeTotalDifficultyTime;
    }
}