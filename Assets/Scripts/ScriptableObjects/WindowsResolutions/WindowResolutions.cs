using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WindowResolutions", menuName = "Settings/WindowResolutions")]
public class WindowResolutions : ScriptableObject
{
    public Vector2 AspectRaio = new Vector2();
    public List<Vector2> Resolutions = new List<Vector2>();

    public string GetAspectRatioSting()
    {
        return $"{AspectRaio.x}x{AspectRaio.y}";
    }

    public string GetResolutionString(int index)
    {
        return $"{Resolutions[index].x}x{Resolutions[index].y}";
    }
}
