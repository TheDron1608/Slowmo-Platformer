using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawn : GenerateOnFinishAllBuildingEnviroment
{
    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        if (generationInfo.Chunk != null && generationInfo.Chunk == generationInfo?.Building?.Enter?.Chunk)
        {
            return null;
        }

        EnemySpawnInfo spawnInfo = SpawnManager.Instance.PickRandomEnemy();

        if (spawnInfo == null || spawnInfo.Enemy == null) return null;

        //Debug.Log(generationInfo.GetSpawnPosition() + " : " + gameObject.GetInstanceID());
        //spawn character
        CharacterComponentsManager newEnemy = generationInfo.GenerateWhere.TrySpawnObject(
            spawnInfo.Enemy.gameObject,
            generationInfo.GetTileSpawnPosition(),
            generationInfo.Building,
            generationInfo.Chunk
            ).First()?.GetComponent<AbstractCharacterComponent>().CharComponents;

        if (newEnemy == null) return new List<GameObject>(0);

        LayerManager.Instance.ChangeZIndexForGameObject(generationInfo.GenerateWhere, newEnemy.gameObject);

        //give weapon
        newEnemy.CharacterHolding.GiveNewHoldable(spawnInfo.Weapon?.PickRandomWeapon());

        //give equipment
        foreach (CharacterEquipmentPart randomEquipment in spawnInfo.Equipment?.PickRandomEquipment() ?? new List<CharacterEquipmentPart>())
        {
            newEnemy.CharacterPartsManager.GiveNewEquipment(randomEquipment);
        }

        return new List<GameObject> { newEnemy.gameObject };
    }
}
