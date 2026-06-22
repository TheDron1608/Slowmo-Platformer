using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public AbstractSoundPlayer SoundOnPause;
    public AbstractSoundPlayer SoundOnUnpause;
    public AbstractSoundPlayer PassiveSoundOnPaused;

    [SerializeField] private Button _defaultSelectedButton;

    private bool _paused = false;
    private float _timeScaleBeforePause = 1f;

    public bool Paused
    {
        get => _paused;
        set
        {
            if (_paused == value || UIManager.Instance.SettingOverlay.IsShown()) return;

            _paused = value;

            if (_paused)
            {
                _timeScaleBeforePause = Time.timeScale;
                TimeManager.Instance.Paused = true;
            }
            else
            {
                TimeManager.Instance.Paused = false;
            }

            if (_paused)
            {
                SoundOnPause.PlaySound();
                PassiveSoundOnPaused.PlaySound(true);
            }
            else
            {
                SoundOnUnpause.PlaySound(false, Vector2.zero);
                PassiveSoundOnPaused.BreakAllSounds();
            }

            gameObject.SetActive(value);
            UIManager.Instance.ModificatorsScreenOverlay.GetModificatorsUI()?.SetPauseModificatorsAligment(value);
            UIManager.Instance.ArtifactModificatorsScreenOverlay.GetModificatorsUI()?.SetPauseModificatorsAligment(value);

            if (CurrentDeviceTracker.GetGamepadIsConnected()) _defaultSelectedButton.Select();
        }
    }
}