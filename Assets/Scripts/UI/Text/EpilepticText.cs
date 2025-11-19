using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class EpilepticText : MonoBehaviour
{
    public float ColorChangeDelay = 0.1f;
    public Color FirstColor;
    public Color SecondColor;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        _text.color = (Time.time % (ColorChangeDelay * 2)) < ColorChangeDelay ? FirstColor : SecondColor;
    }
}
