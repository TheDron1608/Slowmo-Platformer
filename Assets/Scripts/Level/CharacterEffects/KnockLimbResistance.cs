using UnityEngine;

public class KnockLimbResistance : AbstractCharacterLimbEffect
{
    public float KnockMultiplier = 1f;

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && KnockMultiplier == (other as KnockLimbResistance).KnockMultiplier;
    }
}
