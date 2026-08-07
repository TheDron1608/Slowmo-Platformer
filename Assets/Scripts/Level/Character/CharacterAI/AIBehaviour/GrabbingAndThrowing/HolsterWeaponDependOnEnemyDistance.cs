
public class HolsterWeaponDependOnEnemyDistance : AbstractAIGrabbingAndThrowing
{
    public float EnemyDistanceToGrabMelee = 3.5f;

    private void FixedUpdate()
    {
        if (CharComponents.CharacterHolding.CurrentHolsteredHoldObject != null)
        {
            bool isHoldingRanged = CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out RangedWeapon rw) ?? false;
            bool isHoldingGrenade = CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out OnInteractArmGrenade rg) ?? false;
            bool isHolsteringRanged = CharComponents.CharacterHolding.CurrentHolsteredHoldObject?.TryGetComponent(out RangedWeapon hrw) ?? false;
            bool isHolsteringGrenade = CharComponents.CharacterHolding.CurrentHolsteredHoldObject?.TryGetComponent(out OnInteractArmGrenade hrg) ?? false;
            bool isHoldingMelee = CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out MeleeWeapon mw) ?? false;
            bool isHolsteringMelee = CharComponents.CharacterHolding.CurrentHolsteredHoldObject?.TryGetComponent(out MeleeWeapon hmw) ?? false;

            if (isHolsteringGrenade && !isHoldingGrenade)
            {
                CharComponents.CharacterHolding.TrySwapHolsteredWeaponWithCurrent();
            }
            else if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance > EnemyDistanceToGrabMelee)
            {
                if (isHolsteringRanged && !isHoldingRanged && !isHoldingGrenade)
                {
                    CharComponents.CharacterHolding.TrySwapHolsteredWeaponWithCurrent();
                }
            }
            else
            {
                if (isHolsteringMelee && !isHoldingMelee && !isHoldingGrenade)
                {
                    CharComponents.CharacterHolding.TrySwapHolsteredWeaponWithCurrent();
                }
            }
        }
    }
}