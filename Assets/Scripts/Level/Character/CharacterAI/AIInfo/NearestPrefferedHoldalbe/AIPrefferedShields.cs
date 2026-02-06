
public class AIPrefferedShields : AIPrefferedHoldableOrderByDistance
{
    protected override bool PickUpCondition(Holdable holdable)
    {
        return base.PickUpCondition(holdable) && holdable.GetComponent<Shield>() != null;
    }
}
