
public class AIPrefferedMeleeWeapon : AIPrefferedHoldableOrderByDistance
{
    protected override bool PickUpCondition(Holdable holdable)
    {
        return base.PickUpCondition(holdable) && holdable.GetComponent<MeleeWeapon>() != null;
    }
}
