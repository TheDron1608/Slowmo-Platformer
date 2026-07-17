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
        public float GlitchIntencity = 0f;
    }

    public static ScoreManager Instance;

    public int MinCombo = 0;
    public float ResetComboDelay = 5f;
    public float ResetComboDelayOnStartLevel = 10f;
    public List<ComboState> ComboStates = new();
    public Action<ScoreManager> OverrideResetComboEvent = null;

    private Action<ScoreManager> DefaultResetComboEvent = (e) => { e.CurrentCombo = 0; };

    private int _totalScore = 0;
    private int _tradableScore = 0;
    private int _currentCombo = 0;
    private float _comboLastTime;
    private float _currentMultiplier = 1f;
    private ComboState _currentComboState = null;
    private float _resetScoreEncountSpeedMultiplier = 1f;
    private int _lastCombo = 0;

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

    public float ResetScoreEncountSpeedMultiplier
    {
        get => _resetScoreEncountSpeedMultiplier;
        set => _resetScoreEncountSpeedMultiplier = value;
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
            if (_currentCombo < MinCombo) _currentCombo = MinCombo;
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

    public int LastCombo
    {
        get => _lastCombo;
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
        if (CurrentCombo > MinCombo)
        {
            _lastCombo = CurrentCombo;
            OnResetCombo?.Invoke(this, EventArgs.Empty);
            AddScore((int)math.round(CurrentCombo * CurrentMultiplier));
        }
        
        (OverrideResetComboEvent ?? DefaultResetComboEvent)?.Invoke(this);
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 ColorManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        UpdateComboState();

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
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
        _comboLastTime = CurrentCombo > MinCombo ? ResetComboDelayOnStartLevel : 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TargetGlitchIntencity =
                SceneList.GetCurrentSceneIsGameplay() ?
                CurrentComboState.GlitchIntencity : 0f;
        }
    }

    private void Update()
    {
        if (!SceneList.GetCurrentSceneIsGameplay())
        {
            return;
        }

        float newComboLastTime = ComboLastTime - Time.deltaTime * ResetScoreEncountSpeedMultiplier;
        if (newComboLastTime < 0f)
        {
            newComboLastTime = 0f;
            ComboLastTime = newComboLastTime;
            if (CurrentCombo > MinCombo) ResetCombo();
        }
        else
        {
            ComboLastTime = newComboLastTime;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.TargetGlitchIntencity =
                SceneList.GetCurrentSceneIsGameplay() ? 
                CurrentComboState.GlitchIntencity : 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

        Instance = null;
    }
}
