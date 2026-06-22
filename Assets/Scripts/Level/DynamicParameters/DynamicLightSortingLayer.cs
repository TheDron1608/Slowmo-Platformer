using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
[DefaultExecutionOrder(-20)]
public class DynamicLightSortingLayer : MonoBehaviour
{
    private Light2D _lightComponent;
    private int[] _currentValue;

    private void Awake()
    {
        _lightComponent = GetComponent<Light2D>();
    }

    public int[] SortingLayer
    {
        get
        {
            return _currentValue;
        }
        set
        {
            if (value == _currentValue) return;

            _lightComponent.targetSortingLayers = value;

            _currentValue = value;
        }
    }
}
