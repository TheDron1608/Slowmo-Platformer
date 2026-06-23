using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class SoundPlayerOnTextCountDown : MonoBehaviour
{
    public float PitchOnEncountStart = 1f;
    public float PitchOnEncountFinish = 1f;
    public float StartContdownFrom = 999999f;

    [SerializeField] private AbstractSoundPlayer _soundPlayer;
    [SerializeField] private TextMeshProUGUI _textMesh;

    private float? _encountValuePrevFrame = null;
    private float _maxEncountValue;

    private void Start()
    {
        float currentEncountValue;
        if (float.TryParse(_textMesh.text, out currentEncountValue))
        {
            _maxEncountValue = currentEncountValue;
        }
    }

    private void FixedUpdate()
    {
        float currentEncountValue;
        if (float.TryParse(_textMesh.text, out currentEncountValue))
        {
            if (
                currentEncountValue <= StartContdownFrom &&
                (
                    !_encountValuePrevFrame.HasValue ||
                    math.abs(math.ceil(_encountValuePrevFrame.Value) - math.ceil(currentEncountValue)) > 0.5f
                )
                )
            {
                _soundPlayer.Pitch = math.lerp(
                    PitchOnEncountFinish,
                    PitchOnEncountStart,
                    currentEncountValue / math.min(_maxEncountValue, StartContdownFrom)
                    );
                _soundPlayer.PlaySound();
            }

            _encountValuePrevFrame = currentEncountValue;
        }
        else
        {
            Debug.Log("fail parse " + _textMesh.text);
        }
    }
}