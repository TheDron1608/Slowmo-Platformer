using UnityEngine;

public class CharacterHolstering : AbstractCharacterSpecial
{
    public bool TryHolsterCurrentHoldWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null) return false;

        Holdable oldHolsteredWeapon = CharComponents.CharacterHolding.CurrentHolsteredHoldObject;

        CharComponents.CharacterHolding.CurrentHolsteredHoldObject = CharComponents.CharacterHolding.CurrentHoldObject;

        if (oldHolsteredWeapon != null)
        {
            CharComponents.CharacterHolding.CurrentHoldObject = oldHolsteredWeapon;
        }
        else
        {
            CharComponents.CharacterHolding.CurrentHoldObject = null;
        }

        return true;
    }

    public bool TryUnholsterHoldWeapon()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null || 
            CharComponents.CharacterHolding.CurrentHolsteredHoldObject == null
            ) return false;

        CharComponents.CharacterHolding.CurrentHoldObject = CharComponents.CharacterHolding.CurrentHolsteredHoldObject;
        CharComponents.CharacterHolding.CurrentHolsteredHoldObject = null;

        return true;
    }
}