using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using static ButtonOptions;

public class ButtonOnClickChangeLanguage : MonoBehaviour
{
    [SerializeField] ButtonOptions _buttonOptions;

    private void Start()
    {   
        foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
        {
            _buttonOptions.Options.Add(new ButtonOptionsOption(locale.LocaleName));
        }
        _buttonOptions.SetOptionIndex(LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale));
        _buttonOptions.ButtonOptions_OnOptionChanged += ChangeLanguage;
    }

    private void ChangeLanguage(object sender, int e)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[e];
    }
}
