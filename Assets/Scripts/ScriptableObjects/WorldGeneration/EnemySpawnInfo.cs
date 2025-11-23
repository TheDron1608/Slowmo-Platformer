using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnInfo", menuName = "WorldGeneration/EnemySpawnInfo")]
public class EnemySpawnInfo : ScriptableObject
{
    public CharacterComponentsManager Enemy;
    public EnemyEquipmentInfo Equipment;
    public EnemyWeaponInfo Weapon;
    public float Rarity = 1f;
}
