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
        public AudioClip Music = null;
        public int MidstageCursesAmount = 0;
        public float CursesPrice = 10;
        public int CursesAmount = 1;
        public LocalizedString LocalizedName;
        public Material PrimaryEnviromentMaterial = null;
        public Material SecondaryEnviromentMaterial = null;
        public Material BackgroundEnviromentMaterial = null;
        public Material SkyMaterial = null;
        public string ChangeSceneOnStart = "";
    }

    public static DifficultyManager Instance = null;

    public float CursesAmountPerLoopMult = 2;

    [SerializeField] private DifficultyStage[] _initDifficulties = new DifficultyStage[0];

    private LinkedList<DifficultyStage> _difficulties = new();
    private LinkedListNode<DifficultyStage> _currentDifficulty;
    private float _totalDifficultyTime = 0f;
    private float _currentLoopDifficultyTime = 0f;
    private float _currentDifficultyTime = 0f;
    private float _currentDifficultyMidCurseTime = 0f;
    private float _currentCursesAmountMult = 1f;
    private int _loops = 0;
    private float _timeSpeedMultiplier = 1f;
    private float _cursesPickAmountMult = 1f;

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

    public float CursesPickAmountMultiplier
    {
        get => _currentCursesAmountMult;
        set => _currentCursesAmountMult = value;
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
                float cursePrice = CurrentDifficulty.Value.CursesPrice;
                int curseAmount = (int)math.ceil(CurrentDifficulty.Value.CursesAmount * CursesPickAmountMultiplier);

                if (cursePrice > 0f && curseAmount > 0)
                {
                    UIManager.Instance.DifficultyCurseChoiseScreenOverlay.Show(
                        CurrentDifficulty.Value.CursesPrice, 
                        curseAmount
                        );
                }
            }

            ParallaxManager.Instance.SkyMaterial = CurrentDifficulty.Value.SkyMaterial;
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
            CurrentDifficulty.Value.CursesPrice, 
            null
            );

        foreach (AbstractModificator addModificator in addModificators)
        {
            ModificatorsManager.Instance.AddModificator(addModificator, AbstractModificator.ModificatorStatuses.PERMANENT);
        }

        Instance._currentDifficultyMidCurseTime = 0f;
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}