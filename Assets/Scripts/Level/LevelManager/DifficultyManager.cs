using System;
using System.Collections.Generic;
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
        public AudioClip Music = null;
        public int MidstageCursesAmount = 0;
        public float CursesPrice = 10;
        public LocalizedString LocalizedName;
        public Material PrimaryEnviromentMaterial = null;
        public Material SecondaryEnviromentMaterial = null;
        public Material BackgroundEnviromentMaterial = null;
        public string ChangeSceneOnStart = "";
    }

    public static DifficultyManager Instance = null;

    public LinkedList<DifficultyStage> Difficulties = new();
    public float CursesAmountPerLoopMult = 2;

    [SerializeField] private DifficultyStage[] _initDifficulties = new DifficultyStage[0];

    private LinkedListNode<DifficultyStage> _currentDifficulty;
    private float _totalDifficultyTime = 0f;
    private float _currentLoopDifficultyTime = 0f;
    private float _currentDifficultyTime = 0f;
    private float _currentDifficultyMidCurseTime = 0f;
    private float _currentCursesAmountMult = 1f;
    private int _loops = 0;
    private float _timeSpeedMultiplier = 1f;

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
        set
        {
            if (_currentDifficulty == value) return;

            _currentDifficulty = value;
            _currentDifficultyTime = 0f;
        }
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

    public static List<AbstractModificator> GetRandomCurseModificators(float cursePrice, List<AbstractModificator> exludeModificators)
    {
        return ModificatorsManager.Instance.PickRandomModificators(
            AbstractModificator.ModificatorTypes.NEGATIVE,
            cursePrice * MAX_PRICE_REDUCTION,
            cursePrice,
            false,
            true,
            true,
            exludeModificators
            );
    }

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 DifficultyManager instance per scene");
        Instance = this;
        foreach (var initDiff in _initDifficulties) Difficulties.AddLast(initDiff);
        CurrentDifficulty = Difficulties.First;
    }

    private void Update()
    {
        if (
            SceneList.GetCurrentSceneIsGameplay() &&
            SceneManager.GetActiveScene().name != SceneList.GAME_FINISHED &&
            !TimeManager.Instance.Paused &&
            !UIManager.Instance.GameOverScreenOverlay.IsShown() &&
            !UIManager.Instance.IsLoadingScene()
            )
        {
            float multiplierTime = Time.unscaledDeltaTime * TimeSpeedMultiplier;
            _totalDifficultyTime += multiplierTime;
            _currentLoopDifficultyTime += multiplierTime;
            _currentDifficultyTime += multiplierTime;
            _currentDifficultyMidCurseTime += multiplierTime;

            if (
                CurrentDifficulty.Value.MidstageCursesAmount > 0 &&
                _currentDifficultyMidCurseTime > (CurrentDifficulty.Value.Duration / (CurrentDifficulty.Value.MidstageCursesAmount + 1))
                )
            {
                AddMidCurse();
            }
            if (_currentDifficultyTime > CurrentDifficulty.Value.Duration && CurrentDifficulty.Value.Duration >= 0f)
            {
                RaiseUpDifficulty();
            }
        }
    }

    public void RaiseUpDifficulty()
    {
        if (CurrentDifficulty.Next != null)
        {
            CurrentDifficulty = CurrentDifficulty.Next;
            if (CurrentDifficulty.Value.CursesPrice > 0f)
            {
                StartDifficultyCurseChoise(CurrentDifficulty.Value.CursesPrice);
            }
        }

        if (CurrentDifficulty.Value.ChangeSceneOnStart != "")
        {
            UIManager.Instance.LoadSceneWithEffect(CurrentDifficulty.Value.ChangeSceneOnStart);
        }

        _currentDifficultyTime = 0f;
        _currentDifficultyMidCurseTime = 0f;
    }

    public void RaiseUpLoop()
    {
        _loops++;
        _currentCursesAmountMult *= CursesAmountPerLoopMult;
        _currentLoopDifficultyTime = 0f;
    }

    public void AddMidCurse()
    {
        List<AbstractModificator> addModificators = GetRandomCurseModificators(
            CurrentDifficulty.Value.CursesPrice * _currentCursesAmountMult, 
            null
            );

        foreach (AbstractModificator addModificator in addModificators)
        {
            ModificatorsManager.Instance.AddModificator(addModificator, AbstractModificator.ModificatorStatuses.PERMANENT);
        }

        Instance._currentDifficultyMidCurseTime = 0f;
    }

    public void StartDifficultyCurseChoise(float cursePrice)
    {
        UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Show(cursePrice);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}