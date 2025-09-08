using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnInfo", menuName = "WorldGeneration/EnemySpawnInfo")]
public class EnemySpawnInfo : ScriptableObject
{
    public CharacterComponentsManager Enemy;
    public EnemyEquipmentInfo Equipment;
    public float Rarity = 1f;
}
