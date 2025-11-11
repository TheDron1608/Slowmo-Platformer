using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button _defaultSelectedButton;

    private bool _paused = false;

    public bool Paused
    {
        get => _paused;
        set
        {
            _paused = value;
            Time.timeScale = value ? 0f : 1f;
            gameObject.SetActive(value);
            if (CurrentDeviceTracker.GetGamepadIsConnected()) _defaultSelectedButton.Select();
        }
    }


}