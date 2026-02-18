using TMPro;
using UnityEngine;

public class ModificatorVisualInfo : MonoBehaviour
{
    public ModificatorCard Card = null;
    public ModificatorIcon Icon = null;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public bool StrikedDesc = false;

    private void Update()
    {
        if (Card != null)
        {
            Title.text = Card.LocalizedTitle;
            Description.text = Card.LocalizedDescription;
        }
        else if (Icon != null)
        {
            Title.text = Icon.LocalizedTitle;
            Description.text = Icon.LocalizedDescription;
        }

        Description.fontStyle = StrikedDesc ? FontStyles.Strikethrough : FontStyles.Normal;
    }
}