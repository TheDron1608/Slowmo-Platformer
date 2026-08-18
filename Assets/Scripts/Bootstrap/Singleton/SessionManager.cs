using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(10)]
public class SessionManager : MonoBehaviour
{
    public class SessionData
    {
        public int Id;
        public string SaveFilePath;

        public int TotalKills = 0;
        public int TotalDeaths = 0;
        public int TotalObtainedCurses = 0;
        public int TotalPlayTime = 0;
        public float TotalSoldCurses = 0f;
        public List<string> FoundUniqueHoldables = new();
        public string CurrentBossName = null;
        public List<string> CurrentBossModificators = new();
        public string CurrentBossWeapon = null;
        public List<string> UnlockedCharacters = new();
    }

    public class TempSessionData
    {
        public int CurrentKills = 0;
        public int CurrentObtainedCurses = 0;
        public float TotalSoldCurses = 0f;
        public float MaxSoldCurses = 0f;
        public TimeSpan CurrentPlayTime = new TimeSpan(0, 0, 0, 0, 0); //0 seconds
    }

    public static SessionManager Instance;

    public List<SessionData> Sessions;
    public List<PlayerCharacterInfo> DefaultUnlockedCharacters = new();
    public List<PlayerCharacterInfo> TotalCharacters = new();
    public List<Holdable> TotalHoldables = new();

    [SerializeField] private GameObject _tempSessionManagersPrefab;
    private GameObject _tempSessionManagersInstance = null;
    private SessionData _currentSession;
    private TempSessionData _tempSession;
    private PlayerCharacterInfo _currentSelectedPlayer = null;
    private bool _requestSaveSessionBeforeLoadScene = false;

    public event EventHandler CurrentSessionChanged;

    public SessionData CurrentSession
    {
        get => _currentSession;
        set
        {
            ResetTempSessionData();

            if (value != _currentSession)
            {
                if (_tempSessionManagersInstance != null)
                {
                    Destroy(_tempSessionManagersInstance);
                }
                if (value != null)
                {
                    _tempSessionManagersInstance = Instantiate(_tempSessionManagersPrefab);
                    DontDestroyOnLoad(_tempSessionManagersInstance);
                }
            }

            _currentSession = value;
            CurrentSessionChanged?.Invoke(this, EventArgs.Empty);

        }
    }

    public TempSessionData TempSession
    {
        get => _tempSession;
    }

    public PlayerCharacterInfo CurrentSelectedPlayer
    {
        get => _currentSelectedPlayer;
        set
        {
            _currentSelectedPlayer = value;
            InitSelectedPlayer();
        }
    }

    public Holdable GetHoldableByUniqueCode(string holdableCode)
    {
        foreach (Holdable h in TotalHoldables)
        {
            if (h.FindingUniqueCodeName == holdableCode) return h;
        }
        return null;
    }

    public void InitSelectedPlayer()
    {
        SpawnManager.Instance.PlayerCharacter = CurrentSelectedPlayer.PlayerCharacter;
        foreach (AbstractModificator mod in CurrentSelectedPlayer.StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(mod, AbstractModificator.ModificatorStatuses.CHARACTER_DEFAULT);
        }
        SpawnManager.Instance.PlayerCharacterHoldable = CurrentSelectedPlayer.StartHoldable;

        AnalyticsManager.Instance?.RecordEvent(new StartGameAnalyticsEvent(CurrentSelectedPlayer.PlayerCharacter.gameObject.name));
    }

    public bool GetCharacterIsUnlocked(PlayerCharacterInfo character)
    {
        return
            (CurrentSession?.UnlockedCharacters.Contains(character.GetUnlockCharacterJSONName()) ?? false) ||
            DefaultUnlockedCharacters.Contains(character);
    }

    public void RequestSaveSessionBeforeLoadScene()
    {
        _requestSaveSessionBeforeLoadScene = true;
    }

    void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 Instance of SessionManager objects");
        Instance = this;

        ButtonOnClickNewSaveFile.OnNewSaveAdded += ButtonOnClickNewSaveFile_OnNewSaveAdded;
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        UpdateSessions();

        DontDestroyOnLoad(this);
    }

    private void ButtonOnClickNewSaveFile_OnNewSaveAdded(object sender, EventArgs e)
    {
        UpdateSessions();
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if (_requestSaveSessionBeforeLoadScene)
        {
            SaveCurrentSession();
            _requestSaveSessionBeforeLoadScene = false;
        }
    }

    public void UpdateSessions()
    {
        Sessions = GetAllSessionFromJSON();
    }

    private static List<SessionData> GetAllSessionFromJSON()
    {
        List<SessionData> result = new List<SessionData>();

        List<string> savesStr = JSONFileManager.ReadAllFiles(JSONFileManager.Instance.SavesFolder);

        for (int i = 0; i < savesStr.Count; i++)
        {
            result.Add(JsonUtility.FromJson<SessionData>(savesStr[i]));
        }

        return result;
    }

    public void ClearCurrentSession()
    {
        CurrentSession = null;
    }

    public void ApplyTempSessionToCurrentSessionAndSave()
    {
        CurrentSession.TotalKills += TempSession.CurrentKills;
        CurrentSession.TotalObtainedCurses += TempSession.CurrentObtainedCurses;
        CurrentSession.TotalPlayTime += (int)TempSession.CurrentPlayTime.TotalSeconds;
        CurrentSession.TotalSoldCurses += TempSession.TotalSoldCurses;

        SaveCurrentSession();
    }

    public void SaveCurrentSession()
    {
        JSONFileManager.SaveJSON(JSONFileManager.Instance.SavesFolder, JSONFileManager.Instance.SaveFileRootName, CurrentSession.Id, JsonUtility.ToJson(CurrentSession));
    }

    public void ResetTempSessionData()
    {
        _tempSession = new();
    }

    public IEnumerator DestroyTempSession()
    {
        if (_tempSessionManagersInstance != null)
        {
            Destroy(_tempSessionManagersInstance);
        }

        if (!_tempSessionManagersInstance.IsDestroyed()) yield return null;
    }

    public IEnumerator ResetTempSession()
    {
        ResetTempSessionData();

        yield return DestroyTempSession();

        _tempSessionManagersInstance = Instantiate(_tempSessionManagersPrefab);
        DontDestroyOnLoad(_tempSessionManagersInstance);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
