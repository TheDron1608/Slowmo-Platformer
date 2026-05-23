
public class StartGameAnalyticsEvent : Unity.Services.Analytics.Event
{
    public string CharacterName { set { SetParameter("CharacterName", value); } }

    public int TotalCharacters { set { SetParameter("TotalUnlockedCharacters", value); } }

    public StartGameAnalyticsEvent(string characterName) : base("StartGameStats")
    {
        CharacterName = characterName;

        TotalCharacters = 
            SessionManager.Instance.CurrentSession.UnlockedCharacters.Count + 
            SessionManager.Instance.DefaultUnlockedCharacters.Count;
    }
}