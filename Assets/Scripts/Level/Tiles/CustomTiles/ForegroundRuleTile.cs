using UnityEngine;

[CreateAssetMenu(fileName = "ForegroundRuleTile", menuName = "2D/Tiles/CustomTiles/ForegroundRuleTile")]
public class ForegroundRuleTile : RuleTile
{
    public enum ForegroundBehaviourType
    {
        NORMAL,
        STICKY
    }

    public ForegroundBehaviourType BehaviourType;
    public bool ValidAsPlatform = true;
    public int OverrideOrder = 0;
}
