using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawn : SpawnManagerDependedSpawner
{
    public override List<GameObject> Spawn(ZIndexLayer generateWhere, Vector3Int position)
    {
        EnemySpawnInfo spawnInfo = SpawnManager.Instance.PickRandomEnemy();

        if (spawnInfo == null || spawnInfo.Enemy == null) return null;

        //spawn character
        CharacterComponentsManager newEnemy = Instantiate(
            spawnInfo.Enemy,
            transform.position + position,
            spawnInfo.Enemy.transform.rotation,
            generateWhere.CharactersContainer
            );
        LayerManager.Instance.ChangeZIndexForGameObject(generateWhere, newEnemy.gameObject);

        //give weapon
        newEnemy.CharacterHolding.GiveNewHoldable(spawnInfo.Weapon?.PickRandomWeapon());

        //give equipment
        foreach (CharacterEquipmentPart randomEquipment in spawnInfo.Equipment?.PickRandomEquipment()) {
            newEnemy.CharacterPartsManager.GiveNewEquipment(randomEquipment);
        }

        return new List<GameObject> { newEnemy.gameObject };
    }
}
