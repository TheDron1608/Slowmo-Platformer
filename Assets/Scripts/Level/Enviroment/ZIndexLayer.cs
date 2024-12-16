using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class ZIndexLayer : MonoBehaviour
{
    public static List<ZIndexLayer> ZLayers = new();

    public int GetZLayer()
    {
        return gameObject.layer;
    }

    private void Awake()
    {
        ZLayers.Add(this);
    }


    private void OnDestroy()
    {
        ZLayers.Remove(this);
    }
}
