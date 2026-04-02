using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

public class ModificatorLocalization : MonoBehaviour
{
    const char REPLACE_CHAR = '?';

    public LocalizeStringEvent TitleLocalization;
    public LocalizeStringEvent DescriptionLocalization;
    public bool HideTitle = false;
    public bool HideDescription = false;

    private string _localizedTitle;
    private string _localizedDescription;

    public string LocalizedTitle
    {
        get => 
            HideTitle ? 
            _localizedTitle.FilterReplace(REPLACE_CHAR, false, false, true, true, true, true) : 
            _localizedTitle;
        set => _localizedTitle = value;
    }

    public string LocalizedDescription
    {
        get => 
            HideDescription ?
            _localizedDescription.FilterReplace(REPLACE_CHAR, false, false, true, true, true, true) :
            _localizedDescription;
        set => _localizedDescription = value;
    }
}