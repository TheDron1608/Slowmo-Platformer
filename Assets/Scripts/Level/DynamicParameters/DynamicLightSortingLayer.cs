using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Light2D))]
[DefaultExecutionOrder(-20)]
public class DynamicLightSortingLayer : MonoBehaviour
{
    private FieldInfo _sortingLayer;
    private MethodInfo _updateSortingLayer;
    private Light2D _lightComponent;

    private void Awake()
    {
        _sortingLayer = GetComponent<Light2D>().GetType().GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        _updateSortingLayer = GetComponent<Light2D>().GetType().GetMethod("MarkForUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
        _lightComponent = GetComponent<Light2D>();
    }

    public int[] SortingLayer
    {
        get
        {
            return _sortingLayer.GetValue(_lightComponent) as int[];
        }
        set
        {
            if (value == SortingLayer) return;

            _sortingLayer.SetValue(_lightComponent, value);
            _updateSortingLayer.Invoke(_lightComponent, new object[0]);
        }
    }
}
