using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class SaveButton : MonoBehaviour
{
    const float RESET_WARNING_DELAY_SECONDS = 5f;

    public static event EventHandler OnSaveDeleted;

    [SerializeField]
    private TextMeshProUGUI _textContainer;
    [SerializeField]
    private Image _imageContainer;
    [SerializeField]
    private Color _imageDeleteColor;
    [SerializeField]
    private TextMeshProUGUI _warnTextContainer;
    [SerializeField]
    private LocalizeStringEvent _localizedText;

    //localization data
    public string SaveText;
    public string ProgressText;
    public string DeathsText;
    public string PlaytimeText;

    private int _saveDataIndex;
    private bool _warned = false;
    private Color _baseColor;

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



    private void Start()
    {
        _baseColor = _imageContainer.color;
        ButtonOnClickToggleDeleteSaves.OnDeleteSavesChanged += ButtonOnClickToggleDeleteSaves_OnDeleteSavesChanged;
    }

    private void ButtonOnClickToggleDeleteSaves_OnDeleteSavesChanged(object sender, bool e)
    {
        _imageContainer.color = e ? _imageDeleteColor : _baseColor;
    }

    public void LoadData(int saveDataIndex)
    {
        SaveDataIndex = saveDataIndex;
    }

    public SessionManager.SessionData GetSessionData()
    {
        return SessionManager.Instance.Sessions[SaveDataIndex];
    }

    private void UpdateText()
    {
        SessionManager.SessionData sessionData = GetSessionData();

        /*
        (_localizedText.StringReference["SaveId"] as StringVariable).Value = sessionData.Id.ToString();
        (_localizedText.StringReference["ZoneProgress"] as StringVariable).Value = sessionData.FloorProgress.ToString();
        (_localizedText.StringReference["LevelProgress"] as StringVariable).Value = sessionData.LevelProgress.ToString();
        (_localizedText.StringReference["Deaths"] as StringVariable).Value = sessionData.Deaths.ToString();
        (_localizedText.StringReference["Playtime"] as StringVariable).Value = sessionData.PlayTime.ToString("hh':'mm':'ss");
        */
    }

    //called when clicked
    public void OnClick()
    {
        if (!ButtonOnClickToggleDeleteSaves.DeleteSaves)
        {
            SetCurrentSession();
        }
        else
        {
            if (_warned)
            {
                DeleteSave();
            }
            else
            {
                _textContainer.enabled = false;
                _warnTextContainer.enabled = true;
                _warned = true;
                StartCoroutine(ResetWarningAfterDelay());
            }
        }
    }


    public void SetCurrentSession()
    {
        SessionManager.Instance.CurrentSession = GetSessionData();
    }

    public void DeleteSave()
    {
        JSONFileManager.DeleteJSON(JSONFileManager.Instance.SavesFolder, JSONFileManager.Instance.SaveFileRootName, GetSessionData().Id);
        SessionManager.Instance.UpdateSessions();
        OnSaveDeleted?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator ResetWarningAfterDelay()
    {
        yield return new WaitForSeconds(RESET_WARNING_DELAY_SECONDS);

        _textContainer.enabled = true;
        _warnTextContainer.enabled = false;
        _warned = false;
    }

    private void OnDestroy()
    {
        ButtonOnClickToggleDeleteSaves.OnDeleteSavesChanged -= ButtonOnClickToggleDeleteSaves_OnDeleteSavesChanged;
    }
}
