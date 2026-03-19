using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorGenerationPosition : LateGenerateionEnviroment
{
    public class PreGeneratedDoorTempInfo : PreGeneratedEnviromentTempInfo
    {
        public enum DoorGenerationTypes
        {
            CLOSED,
            ZINDEXDOOR,
            NEXTLEVEL,
            SHOP,
            CURSE
        }

        public DoorGenerationTypes DoorType = DoorGenerationTypes.CLOSED;

        public PreGeneratedDoorTempInfo(ZIndexLayer generateWhere, Vector3 offset, ComplexGenerateionEnviroment targetGeneration, BuildingInfo building, ChunkInfo chunk) : base(generateWhere, offset, targetGeneration, building, chunk)
        {
            chunk.DoorGenPositions.Add(this);
        }

        public List<GameObject> Generate(DoorGenerationTypes doorType)
        {
            DoorType = doorType;
            return Generate();
        }

        public override void Remove()
        {
            base.Remove();
            Chunk.DoorGenPositions.Remove(this);
        }
    }

    public float RemoveOtherObjectsRadius = 1f;

    public GameObject ClosedDoor;
    public OnInteractEnterMultiZDoor ZIndexDoor;
    public OnInteractEnterNextLevelDoor NextLevelDoor;
    public OnInteractEnterShopDoor ShopDoor;
    public OnInteractEnterCurse CurseDoor;

    private GameObject GetCurrentTargetGenerationDoor(PreGeneratedDoorTempInfo generationInfo)
    {
        switch (generationInfo.DoorType)
        {
            case PreGeneratedDoorTempInfo.DoorGenerationTypes.CLOSED:
                return ClosedDoor.gameObject;
            case PreGeneratedDoorTempInfo.DoorGenerationTypes.ZINDEXDOOR:
                return ZIndexDoor.gameObject;
            case PreGeneratedDoorTempInfo.DoorGenerationTypes.NEXTLEVEL:
                return NextLevelDoor.gameObject;
            case PreGeneratedDoorTempInfo.DoorGenerationTypes.SHOP:
                return ShopDoor.gameObject;
            case PreGeneratedDoorTempInfo.DoorGenerationTypes.CURSE:
                return CurseDoor.gameObject;
            default:
                throw new UnityException("generationInfo.DoorType is unset or has no valid value; value: " + generationInfo.DoorType);
        }
    }

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        return new PreGeneratedDoorTempInfo(preGenerateWhere, position, this, building, chunk);
    }

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        if (!(generationInfo is PreGeneratedDoorTempInfo doorGenerationInfo)) throw new UnityException("DoorGenPosition.Generate(generationInfo) generationInfo arg must be of type PreGeneratedDoorTempInfo");

        ZIndexLayer layer = generationInfo.GenerateWhere;
        GameObject newDoor = Instantiate(GetCurrentTargetGenerationDoor(doorGenerationInfo), generationInfo.Offset + transform.position, transform.rotation, layer.FurnitureContainer);
        LayerManager.Instance.ChangeZIndexForGameObject(layer, newDoor.gameObject);

        for (int i = 0; i < generationInfo.Chunk.ObjectsInside.Count; i++)
        {
            if (
                Vector2.Distance(generationInfo.Chunk.ObjectsInside[i].transform.position, newDoor.transform.position) <= RemoveOtherObjectsRadius &&
                generationInfo.Chunk.ObjectsInside[i] != newDoor.gameObject
                )
            {
                Destroy(generationInfo.Chunk.ObjectsInside[i].gameObject);
                generationInfo.Chunk.ObjectsInside.RemoveAt(i);
                i--;
            }
        }

        return new List<GameObject> { newDoor };
    }

    public static void GenerateDoorPair(PreGeneratedDoorTempInfo door1, PreGeneratedDoorTempInfo door2)
    {
        OnInteractEnterMultiZDoor newDoor1 = door1.Generate(PreGeneratedDoorTempInfo.DoorGenerationTypes.ZINDEXDOOR).First().GetComponent<OnInteractEnterMultiZDoor>();
        OnInteractEnterMultiZDoor newDoor2 = door2.Generate(PreGeneratedDoorTempInfo.DoorGenerationTypes.ZINDEXDOOR).First().GetComponent<OnInteractEnterMultiZDoor>();

        newDoor1.Exit = newDoor2;
        newDoor2.Exit = newDoor1;
    }
}