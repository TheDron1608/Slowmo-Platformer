using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[DefaultExecutionOrder(7)]
public class CharacterTeam : AbstractCharacterComponent
{
    [SerializeField] private TeamManager.Teams _team;

    public TeamManager.Teams Team
    {
        get => _team;
        set
        {
            if (_team == value) return;

            TeamManager.Instance.GetTeamDataByTeam(_team)?.RemoveTeamMember(this);
            if (_team == ScoreManager.TRACKED_TEAM)
            {
                CharComponents.CharacterAttacking.OnEffectApplied -= TrackedTeamMember_OnEffectApplied;
            }

            TeamManager.Instance.GetTeamDataByTeam(value)?.AddTeamMember(this);
            if (value == ScoreManager.TRACKED_TEAM)
            {
                CharComponents.CharacterAttacking.OnEffectApplied += TrackedTeamMember_OnEffectApplied;
            }

            _team = value;
        }
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        TeamManager.Instance.GetTeamDataByTeam(_team)?.AddTeamMember(this);
        if (_team == ScoreManager.TRACKED_TEAM)
        {
            CharComponents.CharacterAttacking.OnEffectApplied += TrackedTeamMember_OnEffectApplied;
        }
    }

    public bool GetIsAllyToAnotherTeam(CharacterTeam anotherTeam)
    {
        return Team == anotherTeam?.Team;
    }

    public TeamManager.TeamData GetTeamData()
    {
        return TeamManager.Instance?.GetTeamDataByTeam(Team);
    }

    private void TrackedTeamMember_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (
            e.Effect is ILethalEffect &&
            e.Receiver.TryGetComponent(out AbstractCharacterComponent characterReceiver) &&
            !characterReceiver.CharComponents.CharacterHealth.Died
            )
        {
            ScoreManager.Instance.AddCombo();
            ScoreTracker.Instance.AddKill();
        }
    }

    private void OnDestroy()
    {
        TeamManager.Instance.GetTeamDataByTeam(_team)?.RemoveTeamMember(this);
        if (_team == ScoreManager.TRACKED_TEAM)
        {
            CharComponents.CharacterAttacking.OnEffectApplied -= TrackedTeamMember_OnEffectApplied;
        }
    }
}