using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyEquipmentInfo", menuName = "WorldGeneration/EnemyEquipmentInfo")]
public class EnemyEquipmentInfo : ScriptableObject
{
    [Serializable]
    public class EnemySpawnEquipment
    {
        public List<CharacterEquipmentPart> EquipmentPool;
    }

    public List<EnemySpawnEquipment> PossibleEquipment = new();

    public List<CharacterEquipmentPart> PickRandomEquipment()
    {
        return NumberMath.PickRandomItem(PossibleEquipment).EquipmentPool;
    }
}
