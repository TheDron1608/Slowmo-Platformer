
using UnityEngine;

public static class TimeManager
{
    private static float _baseFixedDeltaTime = Time.fixedDeltaTime;

    private static float _currentTimeScaleMultiplier = 1f;
    private static bool _paused = false;

    public static float CurrentTimeScale
    {
        get => _currentTimeScaleMultiplier;
        set
        {
            if (_currentTimeScaleMultiplier != value)
            {
                if (!Paused)
                {
                    Time.timeScale = value;
                    Time.fixedDeltaTime = _baseFixedDeltaTime * value;
                }
                _currentTimeScaleMultiplier = value;
            }
        }
    }

    public static bool Paused
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
                Time.timeScale = _currentTimeScaleMultiplier;
                Time.fixedDeltaTime = _baseFixedDeltaTime * _currentTimeScaleMultiplier;
            }
        }
    }
}
