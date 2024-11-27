using System;
using System.Collections.Generic;
using UnityEngine;

public class SavesButtonsList : MonoBehaviour
{
    public List<SaveButton> SaveButtonsList = new List<SaveButton>();

    [SerializeField]
    private SaveButton _saveButtonInstance;

    [SerializeField]
    private MoveBetweenTwoCoors _cardOnClickmovedObject;
    [SerializeField]
    private GameObject _cardOnClickMovedObjectTarget;

    [SerializeField]
    private List<ButtonOnClickSetCanvasGroupInteractable.CanvasGroupInteractableSet> _cardOnClickSettedCanvasGroupInteractables = new List<ButtonOnClickSetCanvasGroupInteractable.CanvasGroupInteractableSet>();

    [SerializeField]
    private GameObject _cardOnClickSettedSelectedGameObject;

    [SerializeField]
    private ButtonOnClickNewSaveFile _newSaveFileButton;

    private void Awake()
    {
        LoadSaveList();
        ButtonOnClickNewSaveFile.OnNewSaveAdded += ButtonOnClickNewSaveFile_OnNewSaveAdded;
        SaveButton.OnSaveDeleted += SaveButton_OnSaveDeleted;
    }

    private void ButtonOnClickNewSaveFile_OnNewSaveAdded(object sender, EventArgs e)
    {
        UpdateSaveList();
    }

    private void SaveButton_OnSaveDeleted(object sender, EventArgs e)
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
            SetMoveComponent(SaveButtonsList[i]);
        }
    }

    public void SetMoveComponent(SaveButton saveButton)
    {
        if (!saveButton.TryGetComponent<ButtonOnClickMoveObject>(out ButtonOnClickMoveObject moveComponent)) {
            throw new UnityException("ButtonOnClickMoveObject component not found in " + gameObject.name);
        }
        moveComponent.MovingObject = _cardOnClickmovedObject;
        moveComponent.TargetObject = _cardOnClickMovedObjectTarget;

        if (!saveButton.TryGetComponent<ButtonOnClickSetCanvasGroupInteractable>(out ButtonOnClickSetCanvasGroupInteractable setCanvasGroupComponent))
        {
            throw new UnityException("ButtonOnClickSetCanvasGroupInteractable component not found in " + gameObject.name);
        }
        setCanvasGroupComponent.CanvasGroupInteractables = _cardOnClickSettedCanvasGroupInteractables;

        if (!saveButton.TryGetComponent<ButtonOnClickSetSelectedGameObject>(out ButtonOnClickSetSelectedGameObject setSelectedGameObject))
        {
            throw new UnityException("ButtonOnClickSetSelectedGameObject component not found in " + gameObject.name);
        }
        setSelectedGameObject.TargetGameObject = _cardOnClickSettedSelectedGameObject;
    }

    public void UpdateSaveList()
    {
        for (int i = 0; i < SaveButtonsList.Count; i++)
        {
            Destroy(SaveButtonsList[i].gameObject);
        }
        LoadSaveList();
        if (_newSaveFileButton != null)
        {
            _newSaveFileButton.transform.SetAsLastSibling();
            _newSaveFileButton.UpdateHideIfLimitOfSavesReached();
        }
    }

    private void OnDestroy()
    {
        ButtonOnClickNewSaveFile.OnNewSaveAdded -= ButtonOnClickNewSaveFile_OnNewSaveAdded;
        SaveButton.OnSaveDeleted -= SaveButton_OnSaveDeleted;
    }
}
