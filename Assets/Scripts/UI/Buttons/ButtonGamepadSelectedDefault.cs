using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonGamepadSelectedDefault : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup _checkIfCanvasGroundInteractable;



    void OnEnable()
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
