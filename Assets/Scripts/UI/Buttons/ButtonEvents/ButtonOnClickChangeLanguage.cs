using UnityEngine;
using UnityEngine.Localization.Settings;

public class ButtonOnClickChangeLanguage : MonoBehaviour
{
    [SerializeField] ButtonOptions _buttonOptions;

    private void Start()
    {
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            _buttonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(LocalizationSettings.AvailableLocales.Locales[i].LocaleName));

            if (LocalizationSettings.AvailableLocales.Locales[i] == LocalizationSettings.SelectedLocale)
            {
                _buttonOptions.SetOptionIndex(i);
            }
        }

        _buttonOptions.OnOptionChanged += ChangeLanguage;
    }

    private void ChangeLanguage(object sender, int e)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[e];
        PlayerPrefs.SetString("selected-locale", LocalizationSettings.SelectedLocale.Identifier.Code);
    }
}
