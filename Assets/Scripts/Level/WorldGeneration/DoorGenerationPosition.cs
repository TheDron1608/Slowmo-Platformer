using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ChunkConnection;

public class DoorGenerationPosition : LateGenerateionEnviroment
{
    public class PreGeneratedDoorTempInfo : PreGeneratedEnviromentTempInfo
    {
        public PreGeneratedDoorTempInfo(ZIndexLayer generateWhere, Vector3 offset, ComplexGenerateionEnviroment targetGeneration, BuildingInfo building, ChunkInfo chunk) : base(generateWhere, offset, targetGeneration, building, chunk)
        {
            chunk.DoorGenPositions.Add(this);
        }

        public override void Remove()
        {
            base.Remove();
            Chunk.DoorGenPositions.Remove(this);
        }
    }

    public OnInteractEnterMultiZDoor Door;

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        return new PreGeneratedDoorTempInfo(preGenerateWhere, position, this, building, chunk);
    }

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        ZIndexLayer layer = generationInfo.GenerateWhere;
        OnInteractEnterMultiZDoor newDoor = Instantiate(Door, generationInfo.Offset + transform.position, transform.rotation, layer.FurnitureContainer);
        LayerManager.Instance.ChangeZIndexForGameObject(layer, newDoor.gameObject);
        return new List<GameObject> { newDoor.gameObject };
    }

    public static void GenerateDoorPair(PreGeneratedEnviromentTempInfo door1, PreGeneratedEnviromentTempInfo door2)
    {
        OnInteractEnterMultiZDoor newDoor1 = door1.TargetGeneration.Generate(door1).First().GetComponent<OnInteractEnterMultiZDoor>();
        OnInteractEnterMultiZDoor newDoor2 = door2.TargetGeneration.Generate(door2).First().GetComponent<OnInteractEnterMultiZDoor>();

        newDoor1.Exit = newDoor2;
        newDoor2.Exit = newDoor1;
    }
}