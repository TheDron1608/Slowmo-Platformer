using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(ShadowCaster2D))]
[DefaultExecutionOrder(-20)]
public class DynamicShadowCasterSortingLayer : MonoBehaviour
{
    private FieldInfo _sortingLayer;
    private ShadowCaster2D _shadowCasterComponent;
    private int[] _currentvalue;

    private void Awake()
    {
        _sortingLayer = GetComponent<ShadowCaster2D>().GetType().GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);
        _shadowCasterComponent = GetComponent<ShadowCaster2D>();
        _currentvalue = _sortingLayer.GetValue(_shadowCasterComponent) as int[];
    }

    public int[] SortingLayer
    {
        get
        {
            return _currentvalue;
        }
        set
        {
            if (value == _currentvalue) return;

            _sortingLayer.SetValue(_shadowCasterComponent, value);

            _currentvalue = value;
        }
    }
}
