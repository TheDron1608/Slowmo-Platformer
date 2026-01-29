using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public AbstractSoundPlayer SoundOnPause;
    public AbstractSoundPlayer SoundOnUnpause;

    [SerializeField] private Button _defaultSelectedButton;

    private bool _paused = false;
    private float _timeScaleBeforePause = 1f;

    public bool Paused
    {
        get => _paused;
        set
        {
            if (_paused == value) return;

            _paused = value;

            if (_paused)
            {
                _timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = _timeScaleBeforePause;
            }

            if (!_paused) SoundOnUnpause.PlaySound(false, Vector2.zero);

            gameObject.SetActive(value);
            UIManager.Instance.ModificatorsScreenOverlay.GetModificatorsUI().SetPauseModificatorsAligment(value);

            if (_paused) SoundOnPause.PlaySound();

            if (CurrentDeviceTracker.GetGamepadIsConnected()) _defaultSelectedButton.Select();
        }
    }
}