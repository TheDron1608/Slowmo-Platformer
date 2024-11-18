using UnityEngine;
using static ButtonOptions;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem;

public class LoadLastSessionData : MonoBehaviour
{
    private void Start()
    {
        LoadCurrentLocalzation();
        LoadCurrentKeyBinding();
    }

    private void LoadCurrentLocalzation()
    {
        string currentLanguageData = JSONFileManager.ReadJSON(JSONFileManager.Instance.LanguageFileName);
        if (currentLanguageData == "") return;

        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            if (currentLanguageData == LocalizationSettings.AvailableLocales.Locales[i].LocaleName)
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[i];
            }
        }
    }

    private void LoadCurrentKeyBinding()
    {
        string keybindData = JSONFileManager.ReadJSON(JSONFileManager.Instance.ControlsFileName);
        if (keybindData == "") return;

        InputSystem.actions.LoadBindingOverridesFromJson(keybindData);
    }
}
