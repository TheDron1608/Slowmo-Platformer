
using System.Linq;

[AnalyticsEventName("GameOverStats")]
public class GameOverAnalyticsEvent : AbstractAnalyticsEvent
{
    [AnalyticsPropName("PlayerTotalPlaytimeSeconds")]
    public float PlayerTotalPlayTime;

    [AnalyticsPropName("PlayerCharacter")]
    public string PlayerCharacterName;

    [AnalyticsPropName("TotalModPrice")]
    public float TotalModsPrice;

    [AnalyticsPropName("GameSessionTimeSeconds")]
    public float GameSessionTime;

    public GameOverAnalyticsEvent()
    {
        PlayerTotalPlayTime = SessionManager.Instance.Sessions.Sum(e => e.TotalPlayTime);

        PlayerCharacterName = SpawnManager.Instance.PlayerCharacter.gameObject.name;

        TotalModsPrice = ModificatorsManager.Instance.GetTotalModsPrice();

        GameSessionTime = DifficultyManager.Instance.RealtimeTotalDifficultyTime;
    }
}