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
        int iconIndex = _modificatorsIcons.IndexOf(icon);
        if (iconIndex != -1)
        {
            Destroy(_modificatorsIcons[iconIndex].gameObject);
            _modificatorsIcons.RemoveAt(iconIndex);
        }
    }
}