
public class HolsterWeaponDependOnEnemyDistance : AbstractAIGrabbingAndThrowing
{
    public float EnemyDistanceToGrabMelee = 3.5f;

    private void FixedUpdate()
    {
        if (CharComponents.CharacterHolding.CurrentHolsteredHoldObject != null)
        {
            if (_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemyDistance > EnemyDistanceToGrabMelee)
            {
                if (
                    ((!CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out RangedWeapon rw)) ?? true) &&
                    CharComponents.CharacterHolding.CurrentHolsteredHoldObject.TryGetComponent(out RangedWeapon hrw)
                    )
                {
                    CharComponents.CharacterHolding.TrySwapHolsteredWeaponWithCurrent();
                }
            }
            else
            {
                if (
                    ((!CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out MeleeWeapon mw)) ?? true) &&
                    CharComponents.CharacterHolding.CurrentHolsteredHoldObject.TryGetComponent(out MeleeWeapon hmw)
                    )
                {
                    CharComponents.CharacterHolding.TrySwapHolsteredWeaponWithCurrent();
                }
            }
        }
    }
}