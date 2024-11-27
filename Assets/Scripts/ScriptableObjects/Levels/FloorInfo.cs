using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorInfo", menuName = "Levels/FloorInfo")]
public class FloorInfo : ScriptableObject
{
    public int Floor;

    public string FloorName;

    public List<LevelInfo> Levels = new List<LevelInfo>();
}
