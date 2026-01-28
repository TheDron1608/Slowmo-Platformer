using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(7)]
public class CharacterTeam : AbstractCharacterComponent
{
    public TeamManager.Teams Team;

    protected override void OnAwake()
    {
        base.OnAwake();

        TeamManager.Instance.GetTeamDataByTeam(Team).AddTeamMember(this);
    }

    public bool GetIsAllyToAnotherTeam(CharacterTeam anotherTeam)
    {
        return Team == anotherTeam?.Team;
    }

    public TeamManager.TeamData GetTeamData()
    {
        return TeamManager.Instance?.GetTeamDataByTeam(Team);
    }

    private CharacterTeam TryGetTeamFromSender(MonoBehaviour sender)
    {
        if (sender == null || sender.IsDestroyed())
        {
            return null;
        }
        if (sender.TryGetComponent(out AbstractCharacterComponent senderCharacter))
        {
            return senderCharacter.CharComponents.CharacterTeam;
        }
        else if (sender.TryGetComponent(out Holdable senderHoldable))
        {
            return senderHoldable.CurrentHolder?.CharComponents.CharacterTeam;
        }
        else if (sender.TryGetComponent(out AbstractProjectile senderProjectile))
        {
            if (senderProjectile?.Weapon?.TryGetComponent(out Holdable holdableWeapon) ?? false)
            {
                return holdableWeapon?.CurrentHolder?.CharComponents.CharacterTeam;
            }
            else if (senderProjectile?.Weapon?.TryGetComponent(out UnarmedWeapon unarmedWeapon) ?? false)
            {
                return unarmedWeapon?.CharComponents.CharacterTeam;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        TeamManager.Instance?.GetTeamDataByTeam(Team).RemoveTeamMember(this);
    }
}