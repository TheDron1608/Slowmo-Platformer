using UnityEngine;

public class AddCharacterTeammembersSpecialModificator : AbstractCharactersModificator
{
    public AbstractCharacterSpecial Special;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        if (character.CharacterSpecial != null)
        {
            Destroy(character.CharacterSpecial.gameObject);
        }
        character.CharacterSpecial = Instantiate(Special, character.transform);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        if (character.CharacterSpecial != null)
        {
            Destroy(character.CharacterSpecial.gameObject);
        }
    }
}