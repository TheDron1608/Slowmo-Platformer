using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonOnClickSetSelectedGameObject : MonoBehaviour
{
    [SerializeField]
    private GameObject _targetGameObject;

    public void SetSelectedGameObject()
    {
        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            EventSystem.current.SetSelectedGameObject(_targetGameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
