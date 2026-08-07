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
            Holdable giveHoldable = Weapon?.PickRandomWeapon();
            newCharacter.CharComponents.CharacterHolding.GiveNewHoldable(giveHoldable);

            //give holstered weapon
            Holdable extraHoldable = SpawnManager.Instance.PickRandomExtraHoldable();
            if (extraHoldable != null)
            {
                newCharacter.CharComponents.CharacterHolding.HolsterNewHoldable(extraHoldable);
            }
            else if (RandomManager.Instance.ProcRandomBadChance(SpawnManager.Instance.ChanceToGiveCharacterAnyExtraHoldable))
            {
                Holdable filteredWeapon = null;

                //try give melee weapon if main weapon is ranged or ranged weapon if main weapon is melee
                if (giveHoldable.TryGetComponent(out Weapon mainWeapon))
                {
                    if (mainWeapon is RangedWeapon)
                    {
                        filteredWeapon = Weapon?.PickRandomWeapon<MeleeWeapon>();
                    }
                    else if (mainWeapon is MeleeWeapon)
                    {
                        filteredWeapon = Weapon?.PickRandomWeapon<RangedWeapon>();
                    }
                }

                if (filteredWeapon == null)
                {
                    filteredWeapon = Weapon?.PickRandomWeapon();
                }

                newCharacter.CharComponents.CharacterHolding.HolsterNewHoldable(filteredWeapon);
            }

            //give equipment
            foreach (CharacterEquipmentPart randomEquipment in Equipment?.PickRandomEquipment() ?? new List<CharacterEquipmentPart>())
            {
                newCharacter.CharComponents.CharacterPartsManager.GiveNewEquipment(randomEquipment);
            }
        }

        return newCharacter?.CharComponents;
    }
}
