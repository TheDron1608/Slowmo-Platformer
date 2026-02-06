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

        if (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<RangedWeapon>() != null && ClumsyRangedAttack) return true;
        if (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && ClumsyMeleeAttack) return true;
        if (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<Shield>() != null && ClumsyShielding) return true;

        return false;
    }
}
