using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;

public class ScoreManager : MonoBehaviour
{
    const TeamManager.Teams TRACKED_TEAM = TeamManager.Teams.PLAYER;

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

    private int _currentScore = 0;
    private int _currentCombo = 0;
    private float _comboLastTime;
    private float _currentMultiplier = 1f;

    public int CurrentScore
    {
        get => _currentScore;
        set
        {
            _currentScore = value;
            GameplayUIManager.GetInstance()?.Combo.ForceSetScore(value);
        }
    }
    public void AddScore(int value)
    {
        _currentScore += value;
        GameplayUIManager.GetInstance()?.Combo.AddScore(value);
    }

    public int CurrentCombo
    {
        get => _currentCombo;
        set
        {
            _currentCombo = value;
            GameplayUIManager.GetInstance()?.Combo.UpdateCurrentCombo();
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
        set
        {
            _currentMultiplier = value;
            GameplayUIManager.GetInstance()?.Combo.UpdateCurrentMultiplier();
        }
    }

    public void AddCombo()
    {
        CurrentCombo++;
        RestoreComboLastTime();
    }

    public void RestoreComboLastTime()
    {
        ComboLastTime = ResetComboDelay;
    }

    public void ResetCombo()
    {
        if (CurrentCombo > 0)
        {
            AddScore((int)math.round(CurrentCombo * CurrentMultiplier));
        }
        CurrentCombo = 0;
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 ColorManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        TeamManager.Instance.GetTeamDataByTeam(TRACKED_TEAM).OnTeamMemberDidKill += ComboEncounter_OnTeamMemberDidKill;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        _comboLastTime = CurrentCombo > 0 ? ResetComboDelayOnStartLevel : 0f;
    }

    private void ComboEncounter_OnTeamMemberDidKill(object sender, CharacterTeam e)
    {
        AddCombo();
    }

    private void Update()
    {
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
