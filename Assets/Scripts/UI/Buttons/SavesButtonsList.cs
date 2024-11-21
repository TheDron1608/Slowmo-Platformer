using System;
using System.Collections.Generic;
using UnityEngine;

public class SavesButtonsList : MonoBehaviour
{
    public List<SaveButton> SaveButtonsList = new List<SaveButton>();

    [SerializeField]
    private SaveButton _saveButtonInstance;

    private void Awake()
    {
        LoadSaveList();
        ButtonOnClickNewSaveFile.OnNewSaveAdded += ButtonOnClickNewSaveFile_OnNewSaveAdded;
    }

    private void ButtonOnClickNewSaveFile_OnNewSaveAdded(object sender, EventArgs e)
    {
        UpdateSaveList();
    }

    public void LoadSaveList()
    {
        SaveButtonsList.Clear();
        SaveButtonsList = new List<SaveButton>();
        for (int i = 0; i < SessionManager.Instance.Sessions.Count; i++)
        {
            SaveButtonsList.Add(Instantiate(_saveButtonInstance, transform));
            SaveButtonsList[i].LoadData(i);
        }
    }

    public void UpdateSaveList()
    {
        for (int i = 0; i < SaveButtonsList.Count; i++)
        {
            Destroy(SaveButtonsList[i].gameObject);
        }
        LoadSaveList();
    }

    private void OnDestroy()
    {
        ButtonOnClickNewSaveFile.OnNewSaveAdded -= ButtonOnClickNewSaveFile_OnNewSaveAdded;
    }
}
