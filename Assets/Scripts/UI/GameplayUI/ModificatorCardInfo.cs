using TMPro;
using UnityEngine;

public class ModificatorCardInfo : MonoBehaviour
{
    public ModificatorCard Card = null;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public bool StrikedDesc = false;

    private void Update()
    {
        if (Card != null)
        {
            Title.text = Card.LocalizedTitle;
            Description.text = Card.LocalizedDescription;

            Description.fontStyle = StrikedDesc ? FontStyles.Strikethrough : FontStyles.Normal;
        }
    }
}