using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Light2D))]
[DefaultExecutionOrder(-20)]
public class DynamicLightSortingLayer : MonoBehaviour
{
    const float DISTANCE_TO_DISABLE = 50f;

    private FieldInfo _sortingLayer;
    private MethodInfo _updateSortingLayer;
    private Light2D _lightComponent;
    private int[] _currentValue;

    private void Awake()
    {
        _sortingLayer = GetComponent<Light2D>().GetType().GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        _updateSortingLayer = GetComponent<Light2D>().GetType().GetMethod("MarkForUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
        _lightComponent = GetComponent<Light2D>();
        _currentValue = _sortingLayer.GetValue(_lightComponent) as int[];
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

            _sortingLayer.SetValue(_lightComponent, value);
            _updateSortingLayer.Invoke(_lightComponent, new object[0]);

            _currentValue = value;
        }
    }
    /*private void FixedUpdate()
    {
        _lightComponent.enabled = Vector2.Distance(Camera.main.transform.position, transform.position) < DISTANCE_TO_DISABLE && Camera.main.GetComponent<MultiZLayerCamera>().CurrentZLayer == LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }*/
}
