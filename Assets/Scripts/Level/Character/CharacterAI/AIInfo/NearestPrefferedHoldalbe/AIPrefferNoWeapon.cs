
public class AIPrefferNoWeapon : AbstractAIPrefferedHoldable
{
    protected override bool OrderByPattern(Holdable oldHoldable, Holdable newHoldable)
    {
        return false;
    }

    protected override bool PickUpCondition(Holdable holdable)
    {
        return false;
    }
}
