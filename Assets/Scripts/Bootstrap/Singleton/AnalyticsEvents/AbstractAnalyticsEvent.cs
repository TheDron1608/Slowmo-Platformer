
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;

public abstract class AbstractAnalyticsEvent
{
    protected class AnalyticsEventName : Attribute
    {
        public string Name { get; private set; }
        public AnalyticsEventName(string name) => Name = name;
    }

    protected class AnalyticsPropName : Attribute
    {
        public string Name { get; private set; }
        public AnalyticsPropName(string name) => Name = name;
    }

    public void SendEvent()
    {
        string eventName = 
            GetType().GetAttribute<AnalyticsEventName>()?.Name ?? 
            throw new UnityException("AnalyticsEventName attribute is required for class: " + GetType().Name);

        Dictionary<string, object> eventData = new();
        foreach (var field in GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            AnalyticsPropName propAttr = field.GetAttribute<AnalyticsPropName>();

            if (propAttr != null)
            {
                eventData.Add(propAttr.Name, field.GetValue(this));
            }
        }

        AnalyticsResult result = Analytics.CustomEvent(eventName, eventData);

        if (result != AnalyticsResult.Ok && AnalyticsManager.Instance.LogErrors)
        {
            Debug.LogWarning("failed upload type: " + GetType().ToString() + " error: " + result.ToString());
        }
    }
}
