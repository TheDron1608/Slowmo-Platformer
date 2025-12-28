using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.SmartFormat.Utilities;

[RequireComponent(typeof(LocalizeStringEvent), typeof(ModificatorCard))]
public class ModificatorCardLocalizationMultiplierableVariables : MonoBehaviour
{
    [Serializable]
    public class MultiplierableVariable
    {
        public string VariableName;
        public float BaseValue;
        public float ExtraMultiplier = 1f;
    }

    public List<MultiplierableVariable> Variables = new();

    [SerializeField] private LocalizeStringEvent _localizeComponent;

    public void UpdateLocalizedValues()
    {
        foreach (MultiplierableVariable variable in Variables)
        {
            if (_localizeComponent.StringReference.ContainsKey(variable.VariableName))
            {
                if (_localizeComponent.StringReference[variable.VariableName] is FloatVariable)
                {
                    (_localizeComponent.StringReference[variable.VariableName] as FloatVariable).Value = variable.BaseValue * variable.ExtraMultiplier * GetComponent<ModificatorCard>().Multiplier;
                }
                else if (_localizeComponent.StringReference[variable.VariableName] is IntVariable)
                {
                    (_localizeComponent.StringReference[variable.VariableName] as IntVariable).Value = (int)Math.Round(variable.BaseValue * variable.ExtraMultiplier * GetComponent<ModificatorCard>().Multiplier);
                }
                else if (_localizeComponent.StringReference[variable.VariableName] is StringVariable)
                {
                    (_localizeComponent.StringReference[variable.VariableName] as StringVariable).Value = (variable.BaseValue * variable.ExtraMultiplier * GetComponent<ModificatorCard>().Multiplier).ToString();
                }
            }
        }

        _localizeComponent.RefreshString();
    }

    private void Awake()
    {
        UpdateLocalizedValues();
    }
}