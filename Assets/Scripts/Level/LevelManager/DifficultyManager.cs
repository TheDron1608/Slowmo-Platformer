using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-6)]
public class DifficultyManager : MonoBehaviour
{
    const float MAX_PRICE_REDUCTION = 0.9f;

    [Serializable]
    public class DifficultyStage
    {
        public Sprite DifficultyIcon;
        public Sprite MidCurseIcon;
        public float Duration = 60 * 3; //3 minets
        public Sound Music = null;
        public int MidstageCursesAmount = 0;
        public float CursesMinPrice = 0f;
        public float CursesMaxPrice = 0f;
        public int CursesAmount = 1;
        public int OptionsAmount = 3;
        public LocalizedString LocalizedName;
        public Material PrimaryEnviromentMaterial = null;
        public Material SecondaryEnviromentMaterial = null;
        public Material BackgroundEnviromentMaterial = null;
        public Material SkyMaterial = null;
        public string ChangeSceneOnStart = "";

        private string _localizedName = null;

        public float GetDelayBetweenMidCurses()
        {
            return Duration / (MidstageCursesAmount + 1);
        }

        public string GetLocalizedName()
        {
            if (_localizedName == null) _localizedName = LocalizedName.GetLocalizedString();
            return _localizedName;
        }
    }

    public static DifficultyManager Instance = null;

    public float CursesAmountPerLoopMult = 2;

    [SerializeField] private DifficultyStage[] _initDifficulties = new DifficultyStage[0];
    [SerializeField] private float _timeSpeedMultiplier = 1f;

    private LinkedList<DifficultyStage> _difficulties = new();
    private LinkedListNode<DifficultyStage> _currentDifficulty;
    private float _realtimeTotalDifficultyTime = 0f;
    private float _totalDifficultyTime = 0f;
    private float _currentLoopDifficultyTime = 0f;
    private float _currentDifficultyTime = 0f;
    private float _currentDifficultyMidCurseTime = 0f;
    private int _currentDifficultyAddedMidCurses = 0;
    private int _loops = 0;

    public event EventHandler<DifficultyStage> OnDifficultyIncreased;

    public LinkedList<DifficultyStage> Difficulties
    {
        get => _difficulties;
        set
        {
            if (_difficulties == value) return;

            _difficulties = value;
            UpdateCurrentDifficultyDependedOnLoopTime();
        }
    }

    private void UpdateCurrentDifficultyDependedOnLoopTime()
    {
        float totalStageTime = 0f;
        LinkedListNode<DifficultyStage> currentDifficulty = Difficulties.First;
        do
        {
            totalStageTime += currentDifficulty.Value.Duration;
            if (totalStageTime > CurrentLoopDifficultyTime)
            {
                _currentDifficulty = currentDifficulty;
                _currentDifficultyTime = CurrentLoopDifficultyTime - totalStageTime + currentDifficulty.Value.Duration;
                break;
            }
            currentDifficulty = currentDifficulty.Next;
        }
        while (currentDifficulty.Next != null);
    }

    public float RealtimeTotalDifficultyTime
    {
        get => _realtimeTotalDifficultyTime;
    }
    public float TotalDifficultyTime
    {
        get => _totalDifficultyTime;
    }

    public float CurrentLoopDifficultyTime
    {
        get => _currentLoopDifficultyTime;
    }

    public float CurrentDifficultyTime
    {
        get => _currentDifficultyTime;
    }

    public LinkedListNode<DifficultyStage> CurrentDifficulty
    {
        get => _currentDifficulty;
    }

    public int Loops
    {
        get => _loops;
    }

    public float TimeSpeedMultiplier
    {
        get => _timeSpeedMultiplier;
        set => _timeSpeedMultiplier = value;
    }

    public void UpdateDifficultyEnviromentMaterial()
    {
        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                layer.SetEnvromentMaterialDependOnDifficulty(CurrentDifficulty.Value);
            }
        }
        if (ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.SetParallaxMaterialDependedOnDifficulty(CurrentDifficulty.Value);
            ParallaxManager.Instance.SkyMaterial = CurrentDifficulty.Value.SkyMaterial;
        }
    }

    public void ForceRaiseUpDifficulty()
    {
        ForceSkipTime(CurrentDifficulty.Value.Duration - _currentDifficultyTime);
    }

    public void ForceSkipTime(float time)
    {
        _totalDifficultyTime += time;
        _currentLoopDifficultyTime += time;
        _currentDifficultyTime += time;
        _currentDifficultyMidCurseTime += time;
    }

    public void ForceResetDifficulty()
    {
        _totalDifficultyTime = 0f;
        _currentLoopDifficultyTime = 0f;
        _currentDifficultyTime = 0f;
        _currentDifficultyMidCurseTime = 0f;

        SetDifficulty(Difficulties.First);
    }

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 DifficultyManager instance per scene");
        Instance = this;
        Difficulties = NumberMath.ArrayToLinkedList(_initDifficulties);
    }

    private void Start()
    {
        ParallaxManager.Instance.SkyMaterial = CurrentDifficulty.Value.SkyMaterial;
    }

    private void Update()
    {
        if (
            SceneList.GetCurrentSceneIsGameplay() &&
            SceneManager.GetActiveScene().name != SceneList.GAME_FINISHED &&
            !TimeManager.Instance.Paused &&
            !UIManager.Instance.GameOverScreenOverlay.IsShown() &&
            !UIManager.Instance.IsLoadingScene() &&
            CurrentDifficulty.Next != null
            )
        {
            _realtimeTotalDifficultyTime += Time.unscaledDeltaTime;

            float multiplierTime = Time.unscaledDeltaTime * TimeSpeedMultiplier;
            _totalDifficultyTime += multiplierTime;
            _currentLoopDifficultyTime += multiplierTime;
            _currentDifficultyTime += multiplierTime;
            _currentDifficultyMidCurseTime += multiplierTime;

            while (
                CurrentDifficulty.Value.MidstageCursesAmount > _currentDifficultyAddedMidCurses &&
                _currentDifficultyMidCurseTime > CurrentDifficulty.Value.GetDelayBetweenMidCurses()
                )
            {
                AddMidCurse();
                _currentDifficultyMidCurseTime -= CurrentDifficulty.Value.GetDelayBetweenMidCurses();
                _currentDifficultyAddedMidCurses++;
            }

            if (_currentDifficultyTime > CurrentDifficulty.Value.Duration && CurrentDifficulty.Value.Duration >= 0f)
            {
                SetDifficulty(CurrentDifficulty.Next);
                _currentDifficultyTime = 0f;
                _currentDifficultyMidCurseTime = 0f;
                _currentDifficultyAddedMidCurses = 0;
            }
        }
    }

    private void SetDifficulty(LinkedListNode<DifficultyStage> difficulty)
    {
        if (CurrentDifficulty == difficulty) return;

        _currentDifficulty = difficulty;
        _currentDifficultyTime = 0f;

        if (CurrentDifficulty.Value.CursesAmount > 0)
        {
            UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Show(
                CurrentDifficulty.Value.CursesMinPrice,
                CurrentDifficulty.Value.CursesMaxPrice,
                CurrentDifficulty.Value.CursesAmount,
                CurrentDifficulty.Value.OptionsAmount
                );
        }
        else
        {
            UpdateDifficultyEnviromentMaterial();
        }

        if (CurrentDifficulty.Value.ChangeSceneOnStart != "")
        {
            if (SpawnManager.Instance != null && SceneList.GetCurrentSceneIsGameplay())
            {
                List<CharacterTeam> finishedCharacters = TeamManager.Instance.GetTeamDataByTeam(TeamManager.Teams.PLAYER).GetTeamMembers();
                SpawnManager.Instance.FinishGameplay(finishedCharacters.Count > 0 ? finishedCharacters[0] : null, CurrentDifficulty.Value.ChangeSceneOnStart);
            }
            else
            {
                UIManager.Instance.LoadSceneWithEffect(CurrentDifficulty.Value.ChangeSceneOnStart);
            }
        }

        OnDifficultyIncreased?.Invoke(this, CurrentDifficulty.Value);
    }

    private void RaiseUpLoop()
    {
        _loops++;
        _currentLoopDifficultyTime = 0f;
    }

    private void AddMidCurse()
    {
        List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
            AbstractModificator.ModificatorTypes.NEGATIVE,
            CurrentDifficulty.Value.CursesMinPrice,
            CurrentDifficulty.Value.CursesMaxPrice,
            false,
            true,
            true,
            null,
            true
            );

        foreach (AbstractModificator addModificator in addModificators)
        {
            ModificatorsManager.Instance.AddModificator(addModificator, AbstractModificator.ModificatorStatuses.PERMANENT);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}