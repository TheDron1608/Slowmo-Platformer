using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public const TeamManager.Teams TRACKED_TEAM = TeamManager.Teams.PLAYER;

    [Serializable]
    public class ComboState
    {
        public int MinCombo;
        public int MaxCombo;
        public Sprite BgSprite;
        public float Multiplier = 1f;
        public float Shaking = 0f;
    }

    public static ScoreManager Instance;

    public float ResetComboDelay = 5f;
    public float ResetComboDelayOnStartLevel = 10f;
    public List<ComboState> ComboStates = new();

    private int _totalScore = 0;
    private int _tradableScore = 0;
    private int _currentCombo = 0;
    private float _comboLastTime;
    private float _currentMultiplier = 1f;
    private ComboState _currentComboState = null;

    public event EventHandler OnAddedCombo;
    public event EventHandler OnResetCombo;

    public int TotalScore
    {
        get => _totalScore;
    }

    public int TradableScore
    {
        get => _tradableScore;
        set
        {
            _tradableScore = value;
            GameplayUIManager.GetInstance()?.Combo.ForceSetScore(value);
        }
    }
    public void AddScore(int value)
    {
        _tradableScore += value;
        _totalScore += value;
        GameplayUIManager.GetInstance()?.Combo?.AddScore(value);
    }

    public int CurrentCombo
    {
        get => _currentCombo;
        set
        {
            _currentCombo = value;
            UpdateComboState();
            GameplayUIManager.GetInstance()?.Combo.UpdateComboText();
        }
    }

    public float ComboLastTime
    {
        get => _comboLastTime;
        set
        {
            _comboLastTime = value;
            GameplayUIManager.GetInstance()?.Combo.UpdateComboLastTime();
        }
    }

    public float CurrentMultiplier
    {
        get => _currentMultiplier;
        private set
        {
            _currentMultiplier = value;
            GameplayUIManager.GetInstance()?.Combo.UpdateCurrentMultiplier();
        }
    }

    public ComboState CurrentComboState
    {
        get => _currentComboState;
        private set
        {
            _currentComboState = value;
            GameplayUIManager.GetInstance()?.Combo.UpdateComboState();
        }
    }

    public void AddCombo()
    {
        CurrentCombo++;
        RestoreComboLastTime();
        OnAddedCombo?.Invoke(this, EventArgs.Empty);
    }

    public void RestoreComboLastTime()
    {
        ComboLastTime = ResetComboDelay;
    }

    public void ResetCombo()
    {
        if (CurrentCombo > 0)
        {
            OnResetCombo?.Invoke(this, EventArgs.Empty);
            AddScore((int)math.round(CurrentCombo * CurrentMultiplier));
        }
        CurrentCombo = 0;
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 ColorManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        UpdateComboState();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        TeamManager.Instance.GetTeamDataByTeam(TRACKED_TEAM).OnTeamMemberDidKill += ComboEncounter_OnTeamMemberDidKill;
    }

    private void UpdateComboState()
    {
        foreach (ComboState state in ComboStates)
        {
            if (state.MinCombo <= CurrentCombo && state.MaxCombo > CurrentCombo)
            {
                CurrentComboState = state;
                CurrentMultiplier = state.Multiplier;

                return;
            }
        }
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        _comboLastTime = CurrentCombo > 0 ? ResetComboDelayOnStartLevel : 0f;
    }

    private void ComboEncounter_OnTeamMemberDidKill(object sender, TeamManager.TeamData.MemberKillEventArgs e)
    {
        if (!e.Killed.CharComponents.CharacterEffectsReceiver.WasKilledBefore)
        {
            AddCombo();
        }
    }

    private void Update()
    {
        if (!SceneList.GetCurrentSceneIsGameplay())
        {
            return;
        }

        float newComboLastTime = ComboLastTime - Time.deltaTime;
        if (newComboLastTime < 0f)
        {
            newComboLastTime = 0f;
            ComboLastTime = newComboLastTime;
            if (CurrentCombo > 0) ResetCombo();
        }
        else
        {
            ComboLastTime = newComboLastTime;
        }
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.GetTeamDataByTeam(TRACKED_TEAM).OnTeamMemberDidKill -= ComboEncounter_OnTeamMemberDidKill;
        }

        Instance = null;
    }
}
