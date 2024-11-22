using System;
using TMPro;
using UnityEditor.Localization.Editor;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class SaveButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textContaienr;
    [SerializeField]
    private LocalizeStringEvent _localizedText;

    //localization data
    public string SaveText;
    public string ProgressText;
    public string DeathsText;
    public string PlaytimeText;

    private int _saveDataIndex;
    public int SaveDataIndex
    {
        get
        {
            return (_saveDataIndex);
        }
        set
        {
            _saveDataIndex = value;
            UpdateText();
        }
    }

    public void LoadData(int saveDataIndex)
    {
        SaveDataIndex = saveDataIndex;
        UpdateText();
    }

    public SessionManager.SessionData GetSessionData()
    {
        return SessionManager.Instance.Sessions[SaveDataIndex];
    }

    private void UpdateText()
    {
        SessionManager.SessionData sessionData = GetSessionData();

        (_localizedText.StringReference["SaveId"] as StringVariable).Value = sessionData.Id.ToString();
        (_localizedText.StringReference["ZoneProgress"] as StringVariable).Value = sessionData.ZoneProgress.ToString();
        (_localizedText.StringReference["LevelProgress"] as StringVariable).Value = sessionData.LevelProgress.ToString();
        (_localizedText.StringReference["Deaths"] as StringVariable).Value = sessionData.Deaths.ToString();
        (_localizedText.StringReference["Playtime"] as StringVariable).Value = sessionData.PlayTime.ToString("hh':'mm':'ss");
    }
}
