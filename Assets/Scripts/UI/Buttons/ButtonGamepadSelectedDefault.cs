using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonGamepadSelectedDefault : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _checkIfCanvasGroundInteractable;



    void Start()
    {
        UpdateReselect();
        InputSystem.onDeviceChange += inputSystem_OnDevicesChange;
    }

    private void inputSystem_OnDevicesChange(InputDevice device, InputDeviceChange change)
    {
        UpdateReselect();
    }

    private void UpdateReselect()
    {
        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            if (_checkIfCanvasGroundInteractable == null || _checkIfCanvasGroundInteractable.interactable)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= inputSystem_OnDevicesChange;
    }
}
