using System;
using TMPro;
using UnityEngine;

public class SaveButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textContaienr;

    //localization data
    private string _saveText = "Save";
    private string _levelText = "Level";
    private string _deathsText = "Deaths";
    private string _playtimeText = "Playtime";

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
        SessionManager.SessionData currentSessionData = GetSessionData();
        _textContaienr.text =
@$"{_saveText} {currentSessionData.Id}

{_levelText}
{currentSessionData.ZoneProgress}-{currentSessionData.LevelProgress}
{_deathsText}
{currentSessionData.Deaths}
{_playtimeText}
{currentSessionData.PlayTime.ToString("hh':'mm':'ss")}";
    }
}
