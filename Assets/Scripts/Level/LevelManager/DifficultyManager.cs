using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Serializable]
    public class DifficultyStage
    {
        public float Duration = 60 * 3; //3 minets
        public AudioClip Music = null;
        //public int CursesAmount = 1;
        public float CursesPrice = 10;

        public void AddCurses()
        {
            List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
                AbstractModificator.ModificatorTypes.NEGATIVE,
                CursesPrice * Instance._currentCursesAmountMult
                );

            foreach (AbstractModificator addModificator in addModificators )
            {
                ModificatorsManager.Instance.AddModificator(addModificator, AbstractModificator.ModificatorStatuses.PERMANENT);
            }

            Instance._currentDifficultyAddCurseTime = 0f;
        }
    }

    public static DifficultyManager Instance = null;

    public LinkedList<DifficultyStage> Difficulties = new();
    public float CursesAmountPerLoopMult = 2;

    [SerializeField] private DifficultyStage[] _initDifficulties = new DifficultyStage[0];

    private LinkedListNode<DifficultyStage> _currentDifficulty;
    private float _totalDifficultyTime = 0f;
    private float _currentLoopDifficultyTime = 0f;
    private float _currentDifficultyTime = 0f;
    private float _currentDifficultyAddCurseTime = 0f;
    private float _currentCursesAmountMult = 1f;
    private int _loops = 0;

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

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 DifficultyManager instance per scene");
        Instance = this;
        foreach (var initDiff in _initDifficulties) Difficulties.AddLast(initDiff);
        CurrentDifficulty = Difficulties.First;
    }

    private void Update()
    {
        if (
            SceneList.GetCurrentSceneIsGameplay() && 
            !TimeManager.Instance.Paused &&
            !UIManager.Instance.GameOverScreenOverlay.IsShown()
            )
        {
            _totalDifficultyTime += Time.unscaledDeltaTime;
            _currentLoopDifficultyTime += Time.unscaledDeltaTime;
            _currentDifficultyTime += Time.unscaledDeltaTime;
            _currentDifficultyAddCurseTime += Time.unscaledDeltaTime;

            if (_currentDifficultyTime > CurrentDifficulty.Value.Duration)
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
        }
        else
        {
            CurrentDifficulty = Difficulties.First;
            RaiseUpLoop();
        }

        StartDifficultyCurseChoise(CurrentDifficulty.Value.CursesPrice);
        _currentDifficultyTime = 0f;
    }

    public void RaiseUpLoop()
    {
        _loops++;
        _currentCursesAmountMult *= CursesAmountPerLoopMult;
        _currentLoopDifficultyTime = 0f;
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