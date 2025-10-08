using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(ShadowCaster2D))]
[DefaultExecutionOrder(-20)]
public class DynamicShadowCasterSortingLayer : MonoBehaviour
{
    private FieldInfo _sortingLayer;
    private ShadowCaster2D _shadowCasterComponent;

    private void Awake()
    {
        _sortingLayer = GetComponent<ShadowCaster2D>().GetType().GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        _shadowCasterComponent = GetComponent<ShadowCaster2D>();
    }

    public int[] SortingLayer
    {
        get
        {
            return _sortingLayer.GetValue(_shadowCasterComponent) as int[];
        }
        set
        {
            if (_sortingLayer == null) Debug.Log(gameObject.name);
            _sortingLayer.SetValue(_shadowCasterComponent, value);
        }
    }
}
