using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModificatorsUI : MonoBehaviour
{
    [SerializeField] private Transform _modificatorsContainer;
    [SerializeField] private Transform _modificatorTrackTargetsContainer;
    [SerializeField] private Transform _modificatorOnPauseTrackTargetsContainer;
    [SerializeField] private Transform _onPauseContainer;
    [SerializeField] private Transform _modificatorsSpawnPosition;
    [SerializeField] private Transform _selectedModificatorContainer;
    [SerializeField] private TextMeshProUGUI _selectedModificatorTitle;
    [SerializeField] private TextMeshProUGUI _selectedModificatorDesc;

    private List<ModificatorIcon> _modificatorsIcons = new();

    private void OnEnable()
    {
        SetSelectedModificatorInfoEnabled(false);
    }

    public ModificatorIcon AddModificatorIcon(AbstractModificator modifiactor, bool instantly = false)
    {
        ModificatorIcon newIcon = Instantiate(modifiactor.IconInstance, _modificatorsContainer);
        modifiactor.CurrentIcon = newIcon;
        newIcon.CurrentModificator = modifiactor;
        newIcon.DisabledIcon = modifiactor.DisabledModificator;

        if (instantly)
        {
            newIcon.transform.position = UIElementTrackTarget.CreateTrackTarget(_modificatorTrackTargetsContainer, newIcon.transform).transform.position;
        }
        else
        {
            newIcon.transform.position = _modificatorsSpawnPosition.position;
            UIElementTrackTarget.CreateTrackTarget(_modificatorTrackTargetsContainer, newIcon.transform);
        }

        _modificatorsIcons.Add(newIcon);
        return newIcon;
    }

    public void RemoveModificatorIcon(AbstractModificator modificator)
    {
        for (int i = 0; i < _modificatorsIcons.Count; i++)
        {
            if (modificator.IconInstance.ModificatorInstance == _modificatorsIcons[i].ModificatorInstance)
            {
                modificator.CurrentIcon = null;
                Destroy(_modificatorsIcons[i].gameObject);
                _modificatorsIcons.RemoveAt(i);

                break;
            }
        }
    }

    public void SetPauseModificatorsAligment(bool value)
    {
        foreach (
            Transform child in 
            (value ? _modificatorTrackTargetsContainer : _modificatorOnPauseTrackTargetsContainer).GetComponentsInChildren<Transform>()
            )
        {
            if (child.GetComponent<LayoutGroup>() == null)
            {
                child.SetParent(value ? _modificatorOnPauseTrackTargetsContainer.transform : _modificatorTrackTargetsContainer.transform);
            }
        }

        _onPauseContainer.gameObject.SetActive(value);
        _modificatorTrackTargetsContainer.gameObject.SetActive(!value);
    }

    public void SetSelectedModificatorInfoEnabled(bool value)
    {
        _selectedModificatorContainer.gameObject.SetActive(value);
    }

    public void SetSelectedModificatorInfo(string title, string desc, bool strikedDesc)
    {
        SetSelectedModificatorInfoEnabled(false);

        if (CursePickManager.Instance != null)
        {
            CursePickManager.Instance.SetClusterDisplayedDescription(null);
        }

        _selectedModificatorTitle.text = title;
        _selectedModificatorDesc.text = desc;

        _selectedModificatorDesc.fontStyle = strikedDesc ? FontStyles.Strikethrough : FontStyles.Normal;

        SetSelectedModificatorInfoEnabled(true);
    }
}