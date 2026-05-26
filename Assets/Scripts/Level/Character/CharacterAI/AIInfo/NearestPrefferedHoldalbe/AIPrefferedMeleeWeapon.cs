
public class AIPrefferedMeleeWeapon : AIPrefferedHoldableOrderByDistance
{
    protected override bool PickUpCondition(Holdable holdable)
    {
        return base.PickUpCondition(holdable) && holdable.TryGetComponent(out MeleeWeapon mw);
    }
}
