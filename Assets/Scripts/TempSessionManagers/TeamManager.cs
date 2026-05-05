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

        private List<CharacterTeam> _teamMembers = new();

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

    public class OnTeamChangedEventArgs
    {
        public CharacterTeam Character;
        public Teams OldTeam;
        public Teams NewTeam;

        public OnTeamChangedEventArgs(CharacterTeam character, Teams oldTeam, Teams newTeam)
        {
            Character = character;
            OldTeam = oldTeam;
            NewTeam = newTeam;
        }
    }

    public static TeamManager Instance;

    public List<TeamData> TeamDatas = new();

    public event EventHandler<OnTeamChangedEventArgs> OnTeamChanged;    

    public TeamData GetTeamDataByTeam(Teams team)
    {
        return TeamDatas[(int)team];
    }

    public void InvokeTeamChanged(OnTeamChangedEventArgs eventArgs)
    {
        OnTeamChanged?.Invoke(this, eventArgs);
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 TeamManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
