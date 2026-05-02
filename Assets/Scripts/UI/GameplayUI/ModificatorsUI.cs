using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModificatorsUI : MonoBehaviour
{
    public List<AbstractModificator.ModificatorStatuses> AllowContainStatus = new();

    [SerializeField] private Transform _modificatorsContainer;
    [SerializeField] private Transform _modificatorTrackTargetsContainer;
    [SerializeField] private Transform _modificatorOnPauseTrackTargetsContainer;
    [SerializeField] private Transform _onPauseContainer;
    [SerializeField] private Transform _modificatorsSpawnPosition;
    [SerializeField] private ModificatorVisualInfo _cardInfo;

    private List<ModificatorIcon> _modificatorsIcons = new();

    private void OnEnable()
    {
        SetSelectedModificatorInfoEnabled(false);
    }

    public ModificatorIcon AddModificatorIcon(AbstractModificator modifiactor, bool instantly = false)
    {
        if (!AllowContainStatus.Contains(modifiactor.Status)) return null;

        ModificatorIcon newIcon = ModificatorsManager.Instance.CreateModificatorIcon(modifiactor, _modificatorsContainer);

        if (instantly)
        {
            newIcon.transform.position = UIElementTrackTarget.CreateTrackTarget(_modificatorTrackTargetsContainer, newIcon).transform.position;
        }
        else
        {
            newIcon.transform.position = _modificatorsSpawnPosition.position;
            UIElementTrackTarget.CreateTrackTarget(_modificatorTrackTargetsContainer, newIcon);
        }

        _modificatorsIcons.Add(newIcon);
        return newIcon;
    }

    public void RemoveModificatorIcon(AbstractModificator modificator)
    {
        for (int i = 0; i < _modificatorsIcons.Count; i++)
        {
            if (
                _modificatorsIcons[i].CurrentModificator == modificator ||
                _modificatorsIcons[i].CurrentModificator.OriginalModificator == modificator
                )
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
        _cardInfo.gameObject.SetActive(value);
    }

    public void SetSelectedModificatorInfo(ModificatorIcon icon)
    {
        SetSelectedModificatorInfoEnabled(false);

        if (CursePickManager.Instance != null)
        {
            CursePickManager.Instance.SetClusterDisplayedDescription(null);
        }
        if (BlessPickManager.Instance != null)
        {
            BlessPickManager.Instance.SetClusterDisplayedDescription(null);
        }

        _cardInfo.TargetInfo = icon;

        SetSelectedModificatorInfoEnabled(true);
    }
}