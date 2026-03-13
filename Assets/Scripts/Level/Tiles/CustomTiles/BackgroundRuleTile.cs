using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundRuleTile", menuName = "2D/Tiles/CustomTiles/BackgroundRuleTile")]
public class BackgroundRuleTile : RuleTile
{
    public bool CanBeSpilledByFluidParticles = true;
    public bool CanBeOverridedByGridders = false;
}
