using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnInfo", menuName = "WorldGeneration/EnemySpawnInfo")]
public class EnemySpawnInfo : ScriptableObject
{
    public CharacterComponentsManager Enemy;
    public EnemyEquipmentInfo Equipment;
    public EnemyWeaponInfo Weapon;
    public float Rarity = 1f;

    public CharacterComponentsManager SpawnAt(Vector2 position, ZIndexLayer layer)
    {
        AbstractCharacterComponent newCharacter = layer.TrySpawnObject(
            Enemy.gameObject,
            position,
            null,
            null
            ).FirstOrDefault()?.GetComponent<AbstractCharacterComponent>();

        if (newCharacter != null)
        {
            //give weapon
            newCharacter.CharComponents.CharacterHolding.GiveNewHoldable(Weapon?.PickRandomWeapon());

            //give equipment
            foreach (CharacterEquipmentPart randomEquipment in Equipment?.PickRandomEquipment() ?? new List<CharacterEquipmentPart>())
            {
                newCharacter.CharComponents.CharacterPartsManager.GiveNewEquipment(randomEquipment);
            }
        }

        return newCharacter?.CharComponents;
    }
}
