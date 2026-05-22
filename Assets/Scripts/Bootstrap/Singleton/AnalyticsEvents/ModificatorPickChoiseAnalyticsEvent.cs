
using System.Linq;

[AnalyticsEventName("ModificatorPickChoise")]
public class ModificatorPickChoiseAnalyticsEvent : AbstractAnalyticsEvent
{
    [AnalyticsPropName("PlayerTotalPlaytimeSeconds")]
    public float PlayerTotalPlayTime;

    [AnalyticsPropName("PlayerCharacter")]
    public string PlayerCharacterName;

    [AnalyticsPropName("ModificatorName")]
    public string ModName;

    [AnalyticsPropName("IsRejected")]
    public bool IsRejected;

    [AnalyticsPropName("ChoiseType")]
    public string ChoiseType;

    [AnalyticsPropName("TotalModPrice")]
    public float TotalModsPrice;

    [AnalyticsPropName("GameSessionTimeSeconds")]
    public float GameSessionTime;

    public ModificatorPickChoiseAnalyticsEvent(string modName, bool isRejected, string choiseType)
    {
        PlayerTotalPlayTime = SessionManager.Instance.Sessions.Sum(e => e.TotalPlayTime);

        PlayerCharacterName = SpawnManager.Instance.PlayerCharacter.gameObject.name;

        GameSessionTime = DifficultyManager.Instance.RealtimeTotalDifficultyTime;

        TotalModsPrice = ModificatorsManager.Instance.GetTotalModsPrice();

        ModName = modName;
        IsRejected = isRejected;
        ChoiseType = choiseType;
    }
}