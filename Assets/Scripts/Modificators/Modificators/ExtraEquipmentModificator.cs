using System.Collections.Generic;

public class ExtraEquipmentModificator : AbstractCharactersModificator
{
    public EnemyEquipmentInfo Equipment;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        foreach (CharacterEquipmentPart randomEquipment in Equipment?.PickRandomEquipment() ?? new List<CharacterEquipmentPart>())
        {
            character.CharacterPartsManager.GiveNewEquipment(randomEquipment);
        }
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
    }
}