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

        public float GetDelayBetweenMidCurses()
        {
            return Duration / (MidstageCursesAmount + 1);
        }
    }

    public static DifficultyManager Instance = null;

    public float CursesAmountPerLoopMult = 2;

    [SerializeField] private DifficultyStage[] _initDifficulties = new DifficultyStage[0];

    private LinkedList<DifficultyStage> _difficulties = new();
    private LinkedListNode<DifficultyStage> _currentDifficulty;
    private float _realtimeTotalDifficultyTime = 0f;
    private float _totalDifficultyTime = 0f;
    private float _currentLoopDifficultyTime = 0f;
    private float _currentDifficultyTime = 0f;
    private float _currentDifficultyMidCurseTime = 0f;
    private int _currentDifficultyAddedMidCurses = 0;
    private float _currentCursesAmountMult = 1f;
    private int _loops = 0;
    private float _timeSpeedMultiplier = 1f;
    private float _cursesPickAmountMult = 1f;

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
                RaiseUpDifficulty();
                _currentDifficultyTime = 0f;
                _currentDifficultyMidCurseTime = 0f;
                _currentDifficultyAddedMidCurses = 0;
            }
        }
    }

    private void RaiseUpDifficulty()
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
                else
                {
                    UpdateDifficultyEnviromentMaterial();
                }
            }
        }

        if (CurrentDifficulty.Value.ChangeSceneOnStart != "")
        {
            UIManager.Instance.LoadSceneWithEffect(CurrentDifficulty.Value.ChangeSceneOnStart);
        }

        OnDifficultyIncreased?.Invoke(this, CurrentDifficulty.Value);
    }

    private void RaiseUpLoop()
    {
        _loops++;
        _currentCursesAmountMult *= CursesAmountPerLoopMult;
        _currentLoopDifficultyTime = 0f;
    }

    private void AddMidCurse()
    {
        List<AbstractModificator> addModificators = GetRandomCurseModificators(
            CurrentDifficulty.Value.CursesPrice, 
            null
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