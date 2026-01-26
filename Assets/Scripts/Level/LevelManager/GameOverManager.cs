using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    private TeamManager.TeamData _playerTeam;

    private void Start()
    {
        _playerTeam = TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER);
        _playerTeam.OnTeamMemberKilled += _playerTeam_OnTeamMemberKilled;
    }

    private void _playerTeam_OnTeamMemberKilled(object sender, CharacterTeam e)
    {
        foreach (CharacterTeam character in _playerTeam.GetTeamMembers())
        {
            if (!character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()) return;
        }
        UIManager.Instance.GameplayScreenOverlay.Hide();
        UIManager.Instance.DamagedScreenOverlay.Hide();
        UIManager.Instance.GameOverScreenOverlay.Show();
    }
}