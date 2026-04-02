using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class TeamManager : MonoBehaviour
{
    [Serializable]
    public class TeamData
    {
        public class MemberKillEventArgs
        {
            public MemberKillEventArgs(CharacterTeam killed, CharacterTeam killer)
            {
                Killed = killed;
                Killer = killer;
            }

            public CharacterTeam Killed;
            public CharacterTeam Killer;
        }

        public string Name;

        private int _totalKills = 0;
        private int _totalDeaths = 0;
        private List<CharacterTeam> _teamMembers = new();

        public int GetTotalKills() => _totalKills;
        public int GetTotalDeaths() => _totalDeaths;
        public List<CharacterTeam> GetTeamMembers() => _teamMembers;

        public void AddTeamMember(CharacterTeam member)
        {
            _teamMembers.Add(member);
        }
        public void RemoveTeamMember(CharacterTeam member)
        {
            _teamMembers.Remove(member);
        }
    }

    public enum Teams : int
    {
        PLAYER = 0,
        DEFAULT_ENEMY = 1,
        CHASER = 2
    }

    public static TeamManager Instance;

    public List<TeamData> TeamDatas = new();

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
}
