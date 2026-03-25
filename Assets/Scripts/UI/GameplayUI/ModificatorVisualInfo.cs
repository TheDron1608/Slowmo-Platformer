using TMPro;
using UnityEngine;

public class ModificatorVisualInfo : MonoBehaviour
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Status;

    public ModificatorCard Card
    {
        set
        {
            Title.text = value.LocalizedTitle;
            Description.text = value.LocalizedDescription;
            Status.text = AbstractModificator.GetLocalizedStatus(value.Status, value.ModificatorInstance.ModificatorPrice * value.Multiplier);
        }
    }

    public ModificatorIcon Icon
    {
        set
        {
            Title.text = value.LocalizedTitle;
            Description.text = value.LocalizedDescription;
            Status.text = AbstractModificator.GetLocalizedStatus(value.Status, value.ModificatorInstance.ModificatorPrice * value.Multiplier);
        }
    }

    public bool StrikedDesc
    {
        set
        {
            Description.fontStyle = value ? FontStyles.Strikethrough : FontStyles.Normal;
        }
    }
}