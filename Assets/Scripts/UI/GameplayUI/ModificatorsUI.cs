using System.Collections.Generic;
using UnityEngine;

public class ModificatorsUI : MonoBehaviour
{
    [SerializeField] private Transform _modificatorsContainer;
    [SerializeField] private Transform _modificatorTrackTargetsContainer;
    [SerializeField] private Transform _modificatorsSpawnPosition;

    private List<ModificatorIcon> _modificatorsIcons = new();

    public ModificatorIcon AddModificatorIcon(AbstractModificator modifiactor, bool instantly = false)
    {
        ModificatorIcon newIcon = Instantiate(modifiactor.IconInstance, _modificatorsContainer);
        modifiactor.CurrentIcon = newIcon;

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
}