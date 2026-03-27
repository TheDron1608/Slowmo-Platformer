using UnityEngine;

[DefaultExecutionOrder(1)]
public class CharacterLimbPart : CharacterPart
{
    public CharacterPartHealth CharPartHealth;
    public CharacterHitbox CharPartHitbox;
    public Collider2D Collider;

    public void UnequipAllEquipments()
    {
        for (int i = 0; i < CharComponents.CharacterPartsManager.CharacterParts.Count; i++)
        {
            if (
                CharComponents.CharacterPartsManager.CharacterParts[i] is CharacterEquipmentPart equipmentPart &&
                equipmentPart.EquipAtType == PartType
                )
            {
                equipmentPart.TryUnequipPart();
            }
        }
    }

    public void DestroyAllEquipments()
    {
        for (int i = 0; i < CharComponents.CharacterPartsManager.CharacterParts.Count; i++)
        {
            if (
                CharComponents.CharacterPartsManager.CharacterParts[i] is CharacterEquipmentPart equipmentPart &&
                equipmentPart.EquipAtType == PartType
                )
            {
                equipmentPart.DestroyPart();
            }
        }
    }
}
