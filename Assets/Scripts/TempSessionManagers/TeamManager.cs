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
        public string Name;

        private int _totalKills = 0;
        private int _totalDeaths = 0;
        private List<CharacterTeam> _teamMembers = new();

        public int GetTotalKills() => _totalKills;
        public int GetTotalDeaths() => _totalDeaths;
        public List<CharacterTeam> GetTeamMembers() => _teamMembers;

        public void OnCharacterTeamDidKill(CharacterTeam killer)
        {
            OnTeamMemberDidKill?.Invoke(this, killer);
            _totalKills++;
        }
        public void OnCharacterTeamKilled(CharacterTeam killer)
        {
            OnTeamMemberKilled?.Invoke(this, killer);
            _totalDeaths++;
        }

        public event EventHandler<CharacterTeam> OnTeamMemberKilled;
        public event EventHandler<CharacterTeam> OnTeamMemberDidKill;
    }

    public enum Teams : int
    {
        PLAYER = 0,
        DEFAULT_ENEMY = 1
    }

    public static TeamManager Instance;

    public List<TeamData> TeamDatas = new();

    public TeamData GetTeamDataByTeam(Teams team)
    {
        return TeamDatas[(int)team];
    }

    public void OnLevelPreGenerated()
    {
        LayerManager.Instance.OnObjectSpawned += Instance_OnObjectSpawned;
    }
    public void OnLevelFinished()
    {
        LayerManager.Instance.OnObjectSpawned -= Instance_OnObjectSpawned;
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 TeamManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Instance_OnObjectSpawned(object sender, GameObject e)
    {
        if (e.TryGetComponent(out AbstractCharacterComponent character))
        {
            GetTeamDataByTeam(character.CharComponents.CharacterTeam.Team).GetTeamMembers().Add(character.CharComponents.CharacterTeam);
            character.CharComponents.CharacterAttacking.OnEffectApplied += CharacterAttacking_OnEffectApplied;
        }
    }

    private void CharacterAttacking_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (
            e.Effect is ILethalEffect && 
            e.Receiver.TryGetComponent(out AbstractCharacterComponent killedCharacter) &&
            (e.Sender as MonoBehaviour).TryGetComponent(out AbstractCharacterComponent killerCharacter)
            )
        {
            foreach (TeamData teamData in TeamDatas)
            {
                if (GetTeamDataByTeam(killerCharacter.CharComponents.CharacterTeam.Team) == teamData)
                {
                    teamData.OnCharacterTeamDidKill(killerCharacter.CharComponents.CharacterTeam);
                }
                if (GetTeamDataByTeam(killedCharacter.CharComponents.CharacterTeam.Team) == teamData)
                {
                    teamData.OnCharacterTeamKilled(killedCharacter.CharComponents.CharacterTeam);
                }
            }
        }
    }
}
