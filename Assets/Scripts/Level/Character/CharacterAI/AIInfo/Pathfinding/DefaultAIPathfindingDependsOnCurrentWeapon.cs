public class DefaultAIPathfindingDependsOnCurrentWeapon : DefaultAIPathfinding
{
    protected override void OnUpdateInfo()
    {
        if (
            ((CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out MeleeWeapon mw) ?? false) && CharComponents.CharacterClumsyness.ClumsyMeleeAttack) ||
            ((CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out RangedWeapon rw) ?? false) && CharComponents.CharacterClumsyness.ClumsyRangedAttack)
            )
        {
            CanJumpToTarget = false;
        }
        else
        {
            CanJumpToTarget = true;
        }

        base.OnUpdateInfo();
    }
}