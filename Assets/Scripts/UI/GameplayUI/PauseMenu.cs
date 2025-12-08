using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public SoundPlayer SoundOnPause;
    public SoundPlayer SoundOnUnpause;

    [SerializeField] private Button _defaultSelectedButton;

    private bool _paused = false;

    public bool Paused
    {
        get => _paused;
        set
        {
            if (_paused == value) return;

            _paused = value;
            Time.timeScale = value ? 0f : 1f;
            if (!_paused) SoundOnUnpause.PlaySound(false, Vector2.zero);
            gameObject.SetActive(value);
            if (_paused) SoundOnPause.PlaySound();
            if (CurrentDeviceTracker.GetGamepadIsConnected()) _defaultSelectedButton.Select();
        }
    }
}