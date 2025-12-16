using UnityEngine;
using UnityEngine.EventSystems;

public class ModificatorCard : MonoBehaviour
{
    public AbstractModificator Modificator;

    private string _localizedTitle;
    private string _localizedDescription;

    public string LocalizedTitle
    {
        get => _localizedTitle;
        set => _localizedTitle = value;
    }

    public string LocalizedDescription
    {
        get => _localizedDescription;
        set => _localizedDescription = value;
    }
}