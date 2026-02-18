using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableAllButtonsExceptSelf : MonoBehaviour
{
    private List<Selectable> _disabledSelectables = new();

    private void OnEnable()
    {
        foreach (Selectable selectable in FindObjectsByType<Selectable>(FindObjectsSortMode.None))
        {
            if (!GameObjectUtility.GetTransformIsChildOf(selectable.transform, transform))
            {
                selectable.enabled = false;
                _disabledSelectables.Add(selectable);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Selectable selectable in _disabledSelectables)
        {
            selectable.enabled = true;
        }
        _disabledSelectables = new();
    }
}