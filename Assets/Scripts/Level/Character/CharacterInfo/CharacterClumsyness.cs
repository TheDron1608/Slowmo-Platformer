public class CharacterClumsyness : AbstractCharacterComponent
{
    public bool ClumsyMeleeAttack;
    public bool ClumsyRangedAttack;
    public bool ClumsyShielding;
    public bool ClumsyMovement;
    public bool ClumsyJumping;
    public bool ClumsyReloading;

    public bool GetIsClumsyAttackWithCurrentWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null) return false;

        if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rw) && ClumsyRangedAttack) return true;
        if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out MeleeWeapon mw) && ClumsyMeleeAttack) return true;
        if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Shield s) && ClumsyShielding) return true;

        return false;
    }
}
