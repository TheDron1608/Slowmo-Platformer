using System.Collections.Generic;
using UnityEngine;

public class ModificatorsUI : MonoBehaviour
{
    [SerializeField] private Transform _modificatorsContainer;
    [SerializeField] private Transform _modificatorTrackTargetsContainer;
    [SerializeField] private Transform _modificatorsSpawnPosition;

    private List<ModificatorIcon> _modificatorsIcons = new();

    public ModificatorIcon AddModificatorIcon(ModificatorIcon icon, bool instantly = false)
    {
        ModificatorIcon newIcon = Instantiate(icon, _modificatorsContainer);

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

    public void RemoveModificatorIcon(ModificatorIcon icon)
    {
        for (int i = 0; i < _modificatorsIcons.Count; i++)
        {
            if (icon.ModificatorInstance == _modificatorsIcons[i].ModificatorInstance)
            {
                Destroy(_modificatorsIcons[i].gameObject);
                _modificatorsIcons.RemoveAt(i);

                break;
            }
        }
    }
}