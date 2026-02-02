
public class AIPrefferedRangedWeapon : AIPrefferedHoldableOrderByDistance
{
    protected override bool PickUpCondition(Holdable holdable)
    {
        return base.PickUpCondition(holdable) && holdable.GetComponent<RangedWeapon>() != null;
    }
}
