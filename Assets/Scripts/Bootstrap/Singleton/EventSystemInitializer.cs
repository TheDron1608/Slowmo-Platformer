using UnityEngine;
using UnityEngine.InputSystem;

public class EventSystemInitializer : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        InputSystem.actions.Enable();
    }
}