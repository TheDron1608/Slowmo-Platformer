using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyEquipmentInfo", menuName = "WorldGeneration/EnemyEquipmentInfo")]
public class EnemyEquipmentInfo : ScriptableObject
{
    [Serializable]
    public class EnemySpawnEquipment
    {
        public float SpawnChance = 1f;
        public List<CharacterEquipmentPart> EquipmentPool;
    }

    public List<EnemySpawnEquipment> PossibleEquipment = new();
    public List<Holdable> PossibleWeapon;

    public List<CharacterEquipmentPart> PickRandomEquipment()
    {
        List<CharacterEquipmentPart> result = new();

        foreach (EnemySpawnEquipment equipmentInfo in PossibleEquipment)
        {
            if (equipmentInfo.SpawnChance < 1f && UnityEngine.Random.value > equipmentInfo.SpawnChance) continue;

            result.Add(NumberMath.PickRandomItem(equipmentInfo.EquipmentPool));
        }

        return result;
    }

    public Holdable PickRandomWeapon()
    {
        return NumberMath.PickRandomItem(PossibleWeapon);           
    }
}
