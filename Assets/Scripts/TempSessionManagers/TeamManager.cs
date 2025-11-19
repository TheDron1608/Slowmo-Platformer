using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class TeamManager : MonoBehaviour
{
    [Serializable]
    public class TeamData
    {
        private int _totalKills = 0;
        private int _totalDeaths = 0;
        private List<CharacterTeam> _teamMembers = new();
        private List<CharacterTeam> _aliveTeamMembers = new();
        private List<CharacterTeam> _deadTeamMembers = new();

        public int GetTotalKills() => _totalKills;
        public int GetTotalDeaths() => _totalDeaths;
        public List<CharacterTeam> GetTeamMembers() => _teamMembers;
        public List<CharacterTeam> GetAliveTeamMembers() => _aliveTeamMembers; 
        public List<CharacterTeam> GetDeadTeamMembers() => _deadTeamMembers;

        public void AddTeamMember(CharacterTeam teamMember)
        {
            _teamMembers.Add(teamMember);
            (teamMember.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() ? _deadTeamMembers : _aliveTeamMembers).Add(teamMember);

            OnTeamMembersChanged?.Invoke(this, EventArgs.Empty);
            OnTeamMemberAdded?.Invoke(this, teamMember);
        }
        public void RemoveTeamMember(CharacterTeam teamMember)
        {
            _teamMembers.Remove(teamMember);
            _aliveTeamMembers.Remove(teamMember);
            _deadTeamMembers.Remove(teamMember);

            OnTeamMembersChanged?.Invoke(this, EventArgs.Empty);
            OnTeamMemberRemoved?.Invoke(this, teamMember);
        }
        public void SetTeamMemberKilled(CharacterTeam teamMember, CharacterTeam killer)
        {
            if (!_teamMembers.Contains(teamMember)) return;

            _totalDeaths++;
            if (killer != null)
            {
                killer.GetTeamData()._totalKills++;
                killer.GetTeamData().OnTeamMemberDidKill?.Invoke(this, killer);
            }

            _aliveTeamMembers.Remove(teamMember);
            _deadTeamMembers.Add(teamMember);

            _aliveTeamMembers.Remove(teamMember);
            _deadTeamMembers.Add(teamMember);

            OnTeamMembersChanged?.Invoke(this, EventArgs.Empty);
            OnTeamMemberKilled?.Invoke(this, teamMember);
        }
        public void SetTeamMemberRessurected(CharacterTeam teamMember, CharacterTeam ressurector)
        {
            if (!_teamMembers.Contains(teamMember)) return;

            _aliveTeamMembers.Add(teamMember);
            _deadTeamMembers.Remove(teamMember);

            OnTeamMembersChanged?.Invoke(this, EventArgs.Empty);
            OnTeamMemberRessurected?.Invoke(this, teamMember);
        }

        public event EventHandler OnTeamMembersChanged;
        public event EventHandler<CharacterTeam> OnTeamMemberAdded;
        public event EventHandler<CharacterTeam> OnTeamMemberRemoved;
        public event EventHandler<CharacterTeam> OnTeamMemberKilled;
        public event EventHandler<CharacterTeam> OnTeamMemberDidKill;
        public event EventHandler<CharacterTeam> OnTeamMemberRessurected;
    }

    public enum Teams : int
    {
        PLAYER = 0,
        DEFAULT_ENEMY = 1
    }

    public static TeamManager Instance;

    public TeamData[] TeamDatas = new TeamData[]
    {
        new(),
        new()
    };

    public TeamData GetTeamDataByTeam(Teams team)
    {
        return TeamDatas[(int)team];
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 TeamManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
