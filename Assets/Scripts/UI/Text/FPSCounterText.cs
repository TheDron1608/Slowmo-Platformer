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

    private void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();

        _currentAvgFPS = (int)math.round(1f / Time.unscaledDeltaTime);
    }

    private void Update()
    {
        _timeSpent += Time.deltaTime;

        int frameRate = (int)math.round(1f / Time.unscaledDeltaTime);

        _totalNextFPS += frameRate;
        _totalNextFrames++;

        if (_timeSpent > AVG_UPDATE_DELAY_SECONDS)
        {
            _currentAvgFPS = (int)math.round(_totalNextFPS / _totalNextFrames);
            _timeSpent = 0f;
            _totalNextFPS = 0;
            _totalNextFrames = 0;
        }

        _textMeshProUGUI.text = "FPS: " + frameRate.ToString("0") + "\nAVG: " + _currentAvgFPS.ToString("0");

        _textMeshProUGUI.color = 
            (Application.targetFrameRate != -1 ? Application.targetFrameRate : (float)Screen.currentResolution.refreshRateRatio.value) * LOW_FPS_RELATIVE_MAX_AMOUNT <= _currentAvgFPS ?
            NormalFPSColor : LowFPSColor;
    }
}
