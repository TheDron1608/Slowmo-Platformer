using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClickNewSaveFile : MonoBehaviour
{
    const int MAX_SAVE_FILES_LIMIT = 5;

    public static event EventHandler OnNewSaveAdded;

    [SerializeField]
    private Button _button;

    private void Start()
    {
        ButtonOnClickToggleDeleteSaves.OnDeleteSavesChanged += ButtonObClickToggleDeleteSaves_OnDeleteSavesChanged;

        transform.SetAsLastSibling();
        UpdateHideIfLimitOfSavesReached();
    }

    private void ButtonObClickToggleDeleteSaves_OnDeleteSavesChanged(object sender, bool e)
    {
        _button.interactable = !e;
    }

    public void UpdateHideIfLimitOfSavesReached()
    {
        if (JSONFileManager.CountFilesInFolder(JSONFileManager.Instance.SavesFolder) >= MAX_SAVE_FILES_LIMIT)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void CreateNewSaveFile()
    {
        SessionManager.SessionData newSaveData = new SessionManager.SessionData();

        int newSaveDataIndex = 1;
        while (JSONFileManager.GetFileExist(JSONFileManager.Instance.SavesFolder, JSONFileManager.Instance.SaveFileRootName, newSaveDataIndex)) {
            newSaveDataIndex++;
        }

        newSaveData.SaveFilePath = JSONFileManager.JSON_ROOT_FILES_PATH + JSONFileManager.Instance.SavesFolder + "/" + JSONFileManager.Instance.SaveFileRootName + newSaveDataIndex + ".json";
        newSaveData.Id =  newSaveDataIndex;

        string newSaveDataStr = JsonUtility.ToJson(newSaveData);    
        JSONFileManager.SaveJSON(JSONFileManager.Instance.SavesFolder, JSONFileManager.Instance.SaveFileRootName, newSaveDataIndex, newSaveDataStr);

        OnNewSaveAdded?.Invoke(this, EventArgs.Empty);

        transform.SetAsLastSibling();
        UpdateHideIfLimitOfSavesReached();
    }

    private void OnDestroy()
    {
        ButtonOnClickToggleDeleteSaves.OnDeleteSavesChanged -= ButtonObClickToggleDeleteSaves_OnDeleteSavesChanged;
    }
}
