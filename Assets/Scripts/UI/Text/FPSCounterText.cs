using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class FPSCounterText : MonoBehaviour
{
    const float LOW_FPS_RELATIVE_MAX_AMOUNT = 0.9f;
    const float AVG_UPDATE_DELAY_SECONDS = 1f;

    public Color NormalFPSColor = Color.white;
    public Color LowFPSColor = Color.red;

    private TextMeshProUGUI _textMeshProUGUI;
    int _currentAvgFPS = 0;
    int _totalNextFPS = 0;
    private float _totalNextFrames = 0;
    private float _timeSpent = 0;
    private float _minFPS = 0;
    private float _maxFPS = int.MaxValue;

    private void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();

        float defaultFPS = math.round(1f / Time.unscaledDeltaTime);
        _currentAvgFPS = (int)defaultFPS;
        _maxFPS = defaultFPS;
        _minFPS = defaultFPS;
    }

    private void Update()
    {
        _timeSpent += Time.deltaTime;

        int frameRate = (int)math.round(1f / Time.unscaledDeltaTime);

        if (frameRate > _maxFPS) _maxFPS = frameRate;
        if (frameRate < _minFPS) _minFPS = frameRate;

        _totalNextFPS += frameRate;
        _totalNextFrames++;

        _textMeshProUGUI.text =
            "FPS: " + frameRate.ToString("0") +
            "\nAVG: " + _currentAvgFPS.ToString("0") +
            "\nMIN: " + _minFPS.ToString("0") +
            "\nMAX: " + _maxFPS.ToString("0");

        if (_timeSpent > AVG_UPDATE_DELAY_SECONDS)
        {
            _currentAvgFPS = (int)math.round(_totalNextFPS / _totalNextFrames);
            _timeSpent = 0f;
            _totalNextFPS = 0;
            _totalNextFrames = 0;
            _maxFPS = 0;
            _minFPS = int.MaxValue;
        }

        _textMeshProUGUI.color = 
            (Application.targetFrameRate != -1 ? Application.targetFrameRate : (float)Screen.currentResolution.refreshRateRatio.value) * LOW_FPS_RELATIVE_MAX_AMOUNT <= _currentAvgFPS ?
            NormalFPSColor : LowFPSColor;
    }
}
