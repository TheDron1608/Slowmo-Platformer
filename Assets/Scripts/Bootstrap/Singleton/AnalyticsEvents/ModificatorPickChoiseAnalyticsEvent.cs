
using System.Linq;

public class ModificatorPickChoiseAnalyticsEvent : Unity.Services.Analytics.Event
{
    public float PlayerTotalPlayTime { set { SetParameter("PlayerTotalPlaytimeSeconds", value); } }

    public string PlayerCharacterName { set { SetParameter("PlayerCharacter", value); } }

    public string ModName { set { SetParameter("ModificatorName", value); } }

    public bool IsRejected { set { SetParameter("IsRejected", value); } }

    public string ChoiseType { set { SetParameter("ChoiseType", value); } }

    public float TotalModsPrice { set { SetParameter("TotalModPrice", value); } }

    public float GameSessionTime { set { SetParameter("GameSessionTimeSeconds", value); } }

    public ModificatorPickChoiseAnalyticsEvent(string modName, bool isRejected, string choiseType) : base("ModificatorPickChoise")
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