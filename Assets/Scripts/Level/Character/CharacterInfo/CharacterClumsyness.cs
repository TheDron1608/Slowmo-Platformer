public class CharacterClumsyness : AbstractCharacterComponent
{
    public bool ClumsyMeleeAttack;
    public bool ClumsyRangedAttack;
    public bool ClumsyMovement;
    public bool ClumsyJumping;
    public bool ClumsyReloading;

    public bool GetIsClumsyAttackWithCurrentWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null) return false;

        if (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<RangedWeapon>() != null && ClumsyRangedAttack) return true;
        if (CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<MeleeWeapon>() != null && ClumsyMeleeAttack) return true;

        return false;
    }
}
