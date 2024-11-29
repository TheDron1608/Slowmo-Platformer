using UnityEngine;
using static ButtonOptions;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem;

public class LoadLastSessionData : MonoBehaviour
{
    private void Start()
    {
        LoadCurrentWindowOptions();
        LoadCurrentLocalzation();
        LoadCurrentKeyBinding();
        LoadCurrentSoundVolume();
    }

    private void LoadCurrentLocalzation()
    {
        string currentLanguageData = JSONFileManager.ReadJSON(JSONFileManager.Instance.LanguageFileName);
        if (currentLanguageData == null || currentLanguageData == "") return;

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
        if (keybindData == null || keybindData == "") return;

        InputSystem.actions.LoadBindingOverridesFromJson(keybindData);
    }

    private void LoadCurrentWindowOptions()
    {
        string windowDataStr = JSONFileManager.ReadJSON(JSONFileManager.Instance.WindowFileName);
        if (windowDataStr == null || windowDataStr == "") return;

        JSONFileManager.WindowOptionsSaveData windowDataObj = JsonUtility.FromJson<JSONFileManager.WindowOptionsSaveData>(windowDataStr);

        windowDataObj.ApplyOptions();
    }

    private void LoadCurrentSoundVolume()
    {
        SoundManager.Instance.LoadSoundFromJSON();
    }
}
