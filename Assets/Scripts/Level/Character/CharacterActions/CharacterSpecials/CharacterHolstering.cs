using UnityEngine;

public class CharacterHolstering : AbstractCharacterSpecial
{
    public bool TryHolsterCurrentHoldWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null) return false;

        Holdable oldHolsteredWeapon = CharComponents.CharacterHolding.CurrentHolsteredHoldObject;

        CharComponents.CharacterHolding.TryUnholster();
        bool result = CharComponents.CharacterHolding.TryHolster(CharComponents.CharacterHolding.CurrentHoldObject);
        CharComponents.CharacterHolding.ForceGrab(oldHolsteredWeapon);

        return result;
    }

    public bool TryUnholsterHoldWeapon()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null || 
            CharComponents.CharacterHolding.CurrentHolsteredHoldObject == null
            ) return false;

        Holdable holsteredWeapon = CharComponents.CharacterHolding.CurrentHolsteredHoldObject;

        return
            CharComponents.CharacterHolding.TryUnholster() &&
            CharComponents.CharacterHolding.ForceGrab(holsteredWeapon);
    }
}