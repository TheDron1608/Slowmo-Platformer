using UnityEngine;

public class LimbEffectImmunity : AbstractCharacterLimbEffect
{
    public AbstractCharacterEffect ImmuneTo;

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && ImmuneTo == (other as LimbEffectImmunity).ImmuneTo;
    }
}
