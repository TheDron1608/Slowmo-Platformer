using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.CoreUtils;

public class SessionManager : MonoBehaviour
{
    public class SessionData
    {
        public int Id;
        public string SaveFilePath;
        public int LevelProgress = 1;
        public int FloorProgress = 1;
        public int Deaths = 0;
        public TimeSpan PlayTime = new TimeSpan(0, 0, 0); //0 seconds
    }


    public static SessionManager Instance;

    private SessionData _currentSession;

    public SessionData CurrentSession
    {
        get => _currentSession;
        set
        {
            _currentSession = value;
            CurrentSessionChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    public List<SessionData> Sessions;  

    public event EventHandler CurrentSessionChanged;

    void Start()
    {
        if (Instance != null) throw new UnityException("Limit of 1 Instance of SessionManager objects");
        Instance = this;

        ButtonOnClickNewSaveFile.OnNewSaveAdded += ButtonOnClickNewSaveFile_OnNewSaveAdded;
        UpdateSessions();

        DontDestroyOnLoad(this);
    }

    private void ButtonOnClickNewSaveFile_OnNewSaveAdded(object sender, EventArgs e)
    {
        UpdateSessions();
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



    private void OnDestroy()
    {
        Instance = null;
    }
}
