using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FlashingText : MonoBehaviour
{
    public float VisibleDuration = 1f;
    public float InvisibleDuration = 0.5f;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.enabled = (Time.time % (VisibleDuration + InvisibleDuration)) < VisibleDuration;
    }
}
