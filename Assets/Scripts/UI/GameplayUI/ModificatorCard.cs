using UnityEngine;
using UnityEngine.EventSystems;

public class ModificatorCard : MonoBehaviour
{
    public AbstractModificator ModificatorInstance;

    private float _multiplier = 1f;
    private string _localizedTitle;
    private string _localizedDescription;

    public float Multiplier
    {
        get => _multiplier;
        set
        {
            _multiplier = value;
            if (TryGetComponent(out ModificatorLocalizationMultiplierableVariables localizedVars))
            {
                localizedVars.UpdateLocalizedValues();
            }
        }
    }

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