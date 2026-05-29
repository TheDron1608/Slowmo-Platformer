
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance = null;

    const float SLOWMO_OVERLAY_APPEAR_SPEED = 10f;
    const float MIN_TIME_SCALE = 0.05f;

    public float TempSlowTimeDecaySpeed = 2f;

    private float _baseFixedDeltaTime;
    private float _currentTimeScaleMultiplier = 1f;
    private bool _paused = false;
    private float _tempSlowTimeLeft = 0f;
    private bool _isLoadingProcessTimeStop = false;

    public bool IsLoadingProcessTimeStop
    {
        get => _isLoadingProcessTimeStop;
        set => _isLoadingProcessTimeStop = value;
    }

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 TimeManager instance per scene");

        Instance = this;
        _baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        UpdateSlowmoOverlay();
    }

    private void UpdateSlowmoOverlay()
    {
        if (SceneList.GetCurrentSceneIsGameplay())
        {
            if (!Paused)
            {
                _tempSlowTimeLeft -= Time.unscaledDeltaTime * TempSlowTimeDecaySpeed;
                if (_tempSlowTimeLeft < 0f) _tempSlowTimeLeft = 0f;

                float totalScale = GetTotalTimeScale();

                Time.timeScale = totalScale;
                Time.fixedDeltaTime = _baseFixedDeltaTime * totalScale;
            }

            UIManager.Instance.SlowmoOverlay.Show();

            TrySetSlowmoOverlayFill(math.lerp(
                UIManager.Instance.SlowmoOverlay.FillAmount,
                NumberMath.LimitFloatBetweenZeroAndOne(_tempSlowTimeLeft),
                Time.unscaledDeltaTime * SLOWMO_OVERLAY_APPEAR_SPEED
                ));
        }
        else
        {
            UIManager.Instance.SlowmoOverlay.Hide();
        }
    }

    public void TryTemporalSlowTime(float value)
    {
        if (_tempSlowTimeLeft < value)
        {
            _tempSlowTimeLeft = value;
        }
    }

    public float CurrentTimeScale
    {
        get => _currentTimeScaleMultiplier;
        set
        {
            if (_currentTimeScaleMultiplier != value)
            {
                if (!Paused)
                {
                    float totalScale = GetTotalTimeScale();

                    Time.timeScale = totalScale;
                    Time.fixedDeltaTime = _baseFixedDeltaTime * totalScale;
                }
                _currentTimeScaleMultiplier = value;
            }
        }
    }

    public float TempSlowTimeLeft
    {
        get => _tempSlowTimeLeft;
    }

    public float GetTotalTimeScale()
    {
        return math.max(CurrentTimeScale * (1f - NumberMath.LimitFloatBetweenMinusOneAndOne(_tempSlowTimeLeft)), MIN_TIME_SCALE);
    }

    public bool Paused
    {
        get => _paused;
        set
        {
            if (_paused == value) return;

            _paused = value;
            if (_paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                float totalScale = GetTotalTimeScale();

                Time.timeScale = totalScale;
                Time.fixedDeltaTime = _baseFixedDeltaTime * totalScale;
            }
        }
    }

    private void TrySetSlowmoOverlayFill(float value)
    {
        if (UIManager.Instance != null && UIManager.Instance.SlowmoOverlay.IsShown())
        {
            UIManager.Instance.SlowmoOverlay.FillAmount = value;
        }
    }

    private void OnDestroy()
    {
        TrySetSlowmoOverlayFill(0f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _baseFixedDeltaTime;
        Instance = null;
    }
}
