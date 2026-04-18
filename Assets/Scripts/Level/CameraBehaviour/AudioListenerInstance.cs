using System;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class AudioListenerInstance : MonoBehaviour
{
    public static AudioListenerInstance Instance;

    public event EventHandler OnDestroyed;

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("limit of 1 AudioListenerInstance per scene");

        Instance = this;
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this, EventArgs.Empty);
        Instance = null;
    }
}