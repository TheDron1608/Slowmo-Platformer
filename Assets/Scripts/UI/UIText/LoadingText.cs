using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{   
    public float AddCharDurationSeconds =.75f;
    public bool isAddingChars = true;
    public char addedChar = '.';

    private TextMeshProUGUI _textMeshProUGUI;

    const int MAX_CHARS = 4096;

    private void Awake()
    {
        _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    protected void Start()
    {
        StartCoroutine(WaitAndAddChar());
    }

    private IEnumerator WaitAndAddChar()
    {
        yield return new WaitForSeconds(AddCharDurationSeconds);

        if (_textMeshProUGUI.text.Length < MAX_CHARS)
        {
            _textMeshProUGUI.text += addedChar;
        }

        yield return new WaitUntil(() => isAddingChars); //loops if isAddingChars is true
        StartCoroutine(WaitAndAddChar());
    }
}
