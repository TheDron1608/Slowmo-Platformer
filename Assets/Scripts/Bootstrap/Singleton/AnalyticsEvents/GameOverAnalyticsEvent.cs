
using System.Linq;

public class GameOverAnalyticsEvent : Unity.Services.Analytics.Event
{
    public float PlayerTotalPlayTime { set { SetParameter("PlayerTotalPlaytimeSeconds", value); } }

    public string PlayerCharacterName { set { SetParameter("PlayerCharacter", value); } }

    public float TotalModsPrice { set { SetParameter("TotalModPrice", value); } }

    public float GameSessionTime { set { SetParameter("GameSessionTimeSeconds", value); } }

    public GameOverAnalyticsEvent() : base("GameOverStats")
    {
        PlayerTotalPlayTime = SessionManager.Instance.Sessions.Sum(e => e.TotalPlayTime);

        PlayerCharacterName = SpawnManager.Instance.PlayerCharacter.gameObject.name;

        TotalModsPrice = ModificatorsManager.Instance.GetTotalModsPrice();

        GameSessionTime = DifficultyManager.Instance.TotalDifficultyTime;
    }
}