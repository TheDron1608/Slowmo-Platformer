using UnityEngine;

public class KnockResistance : AbstractCharacterEffect
{
    public float KnockMultiplier = 1f;

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && KnockMultiplier == (other as KnockResistance).KnockMultiplier;
    }
}
