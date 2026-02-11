using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class FPSCounterText : MonoBehaviour
{
    const string FPS_PREFIX = "FPS: ";
    const float LOW_FPS_RELATIVE_MAX_AMOUNT = 0.9f;

    public Color NormalFPSColor = Color.white;
    public Color LowFPSColor = Color.red;

    private TextMeshProUGUI _textMeshProUGUI;

    private void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        int frameRate = (int)math.round(1f / Time.unscaledDeltaTime);
        _textMeshProUGUI.text = FPS_PREFIX +  frameRate.ToString("0");
        
        _textMeshProUGUI.color = 
            Application.targetFrameRate * LOW_FPS_RELATIVE_MAX_AMOUNT <= frameRate ?
            NormalFPSColor : LowFPSColor;
    }
}
