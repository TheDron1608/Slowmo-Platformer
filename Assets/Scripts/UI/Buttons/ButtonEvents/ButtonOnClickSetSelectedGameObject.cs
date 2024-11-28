using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonOnClickSetSelectedGameObject : MonoBehaviour
{
    [SerializeField]
    public GameObject TargetGameObject;

    public void SetSelectedGameObject()
    {
        if (ButtonOnClickToggleDeleteSaves.DeleteSaves) return;

        if (CurrentDeviceTracker.GetGamepadIsConnected())
        {
            EventSystem.current.SetSelectedGameObject(TargetGameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
