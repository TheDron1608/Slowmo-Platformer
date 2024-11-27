using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "FloorInfo", menuName = "Levels/FloorInfo")]
public class FloorInfo : ScriptableObject
{
    public int Floor;

    public LocalizedString LocalizedName;

    public List<LevelInfo> Levels = new List<LevelInfo>();

    public string Name()
    {
        return LocalizedName.GetLocalizedString();
    }
}
