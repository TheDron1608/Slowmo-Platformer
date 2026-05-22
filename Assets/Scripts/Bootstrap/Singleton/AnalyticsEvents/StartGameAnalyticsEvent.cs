[AnalyticsEventName("StartGameStats")]
public class StartGameAnalyticsEvent : AbstractAnalyticsEvent
{
    [AnalyticsPropName("CharacterName")]
    public string CharacterName;

    [AnalyticsPropName("TotalUnlockedCharacters")]
    public int TotalCharacters;

    public StartGameAnalyticsEvent(string characterName)
    {
        CharacterName = characterName;

        TotalCharacters = 
            SessionManager.Instance.CurrentSession.UnlockedCharacters.Count + 
            SessionManager.Instance.DefaultUnlockedCharacters.Count;
    }
}