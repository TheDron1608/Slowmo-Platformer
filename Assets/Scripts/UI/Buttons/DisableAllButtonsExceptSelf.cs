using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableAllButtonsExceptSelf : MonoBehaviour
{
    private List<Button> _disabledButtons = new();

    private void OnEnable()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            if (!GameObjectUtility.GetTransformIsChildOf(button.transform, transform))
            {
                button.enabled = false;
                _disabledButtons.Add(button);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Button button in _disabledButtons)
        {
            button.enabled = true;
        }
        _disabledButtons = new();
    }
}