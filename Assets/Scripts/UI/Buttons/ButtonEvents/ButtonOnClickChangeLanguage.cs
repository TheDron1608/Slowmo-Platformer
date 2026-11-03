using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ButtonOnClickChangeLanguage : MonoBehaviour
{
    public bool AllowAutoTranslatedLocales = false;

    [SerializeField] ButtonOptions _buttonOptions;

    private List<Locale> _filteredLocales = new();

    private void Start()
    {
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            if (
                AllowAutoTranslatedLocales ||
                LocalizationSettings.AvailableLocales.Locales[i].Metadata.MetadataEntries.Any(e => e is IsAutoTranslatedLocale)
                )
            {
                continue;
            }

            _filteredLocales.Add(LocalizationSettings.AvailableLocales.Locales[i]);

            _buttonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(LocalizationSettings.AvailableLocales.Locales[i].LocaleName));

            if (LocalizationSettings.AvailableLocales.Locales[i] == LocalizationSettings.SelectedLocale)
            {
                _buttonOptions.SetOptionIndex(_filteredLocales.Count - 1);
            }
        }

        _buttonOptions.OnOptionChanged += ChangeLanguage;
    }

    private void ChangeLanguage(object sender, int e)
    {
        LocalizationSettings.SelectedLocale = _filteredLocales[e];
        PlayerPrefs.SetString("selected-locale", LocalizationSettings.SelectedLocale.Identifier.Code);
    }
}
