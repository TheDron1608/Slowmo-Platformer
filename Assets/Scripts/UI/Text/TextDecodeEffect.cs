using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class TextDecodeEffect : MonoBehaviour
{
    const float REVAL_CHARS_PER_SECOND = 32.5f;
    const float MAX_REVAL_DURATION_SECONDS = 2f;
    private readonly char[] RANDOM_CHARS = { '.', ',', '\'', '\"', '\\', '/', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '-', '+', '=', '~', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

    private TextMeshProUGUI _text;
    private string _originalText = "";
    private string _currentText = "";
    private int _currentTextProgress = 0;

    private void Awake()
    {
        if (!TryGetComponent(out _text)) throw new UnityException("TextMeshProUGUI component not found");
        _originalText = _text.text;
    }

    private void OnEnable()
    {
        _currentTextProgress = 0;
        StartCoroutine(UnscaledUpdateLoop());
    }

    private IEnumerator UnscaledUpdateLoop()
    {
        while (true)
        {
            if (_currentText != _text.text)
            {
                _originalText = _text.text;
                _currentTextProgress = 0;
                UpdateText();
            }
            else if (_currentTextProgress < _originalText?.Length)
            {
                _currentTextProgress++;
                UpdateText();
            }

            yield return new WaitForSecondsRealtime(math.min(1f /  REVAL_CHARS_PER_SECOND, MAX_REVAL_DURATION_SECONDS / (_originalText?.Length ?? 1f)));
        }
    }

    private void UpdateText()
    {
        if (_originalText == null) return;

        string result = _originalText.Substring(0, math.min(_currentTextProgress, _originalText.Length));
        for (int i = 0; i < _originalText.Length - _currentTextProgress; i++)
        {
            if (_originalText[_currentTextProgress + i] == '\n')
            {
                result += '\n';
            }
            else if (_originalText[_currentTextProgress + i] == ' ')
            {
                result += ' ';
            }
            else
            {
                result += NumberMath.PickRandomItem(RANDOM_CHARS);
            }
        }
        _currentText = result;
        _text.text = result;
    }
}