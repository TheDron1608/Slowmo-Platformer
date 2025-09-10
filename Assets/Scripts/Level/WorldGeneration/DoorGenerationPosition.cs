using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorGenerationPosition : LateGenerateionEnviroment
{
    public OnInteractEnterMultiZDoor Door;
    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        ZIndexLayer layer = generationInfo.GenerateWhere;
        OnInteractEnterMultiZDoor newDoor = Instantiate(Door, generationInfo.Offset + transform.position, transform.rotation, layer.FurnitureContainer);
        layer.UpdateLayerForGameObject(newDoor.gameObject);
        return new List<GameObject> { newDoor.gameObject };
    }

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        PreGeneratedEnviromentTempInfo result = base.PreGenerate(preGenerateWhere, position, building, chunk);
        chunk.DoorGenPositions.Add(result);
        return result;
    }
    public static void GenerateDoorPair(PreGeneratedEnviromentTempInfo door1, PreGeneratedEnviromentTempInfo door2)
    {
        OnInteractEnterMultiZDoor newDoor1 = door1.TargetGeneration.Generate(door1).First().GetComponent<OnInteractEnterMultiZDoor>();
        OnInteractEnterMultiZDoor newDoor2 = door2.TargetGeneration.Generate(door1).First().GetComponent<OnInteractEnterMultiZDoor>();

        newDoor1.Exit = newDoor2;
        newDoor2.Exit = newDoor1;
    }
}