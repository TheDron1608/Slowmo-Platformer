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
        public int ZoneProgress = 1;
        public int Deaths = 0;
        public TimeSpan PlayTime = new TimeSpan(0, 0, 0); //0 seconds
    }


    public static SessionManager Instance;

    public SessionData CurrentSession;
    public List<SessionData> Sessions;  

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

    private void UpdateSessions()
    {
        Sessions = GetAllSessionFromJSON();
    }


    private static List<SessionData> GetAllSessionFromJSON()
    {
        List<SessionData> result = new List<SessionData>();

        string[] savesStr = JSONFileManager.ReadAllFiles(JSONFileManager.Instance.SavesFolder);

        for (int i = 0; i < savesStr.Length; i++)
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
