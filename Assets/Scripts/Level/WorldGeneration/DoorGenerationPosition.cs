using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorGenerationPosition : LateGenerateionEnviroment
{
    public OnInteractEnterMultiZDoor Door;
    public override List<GameObject> Generate()
    {
        OnInteractEnterMultiZDoor newDoor = Instantiate(Door, transform.position, transform.rotation, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform);
        LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForGameObject(newDoor.gameObject);
        return new List<GameObject> { newDoor.gameObject };
    }
    public static void GenerateDoorPair(DoorGenerationPosition door1, DoorGenerationPosition door2)
    {
        OnInteractEnterMultiZDoor newDoor1 = door1.Generate().First().GetComponent<OnInteractEnterMultiZDoor>();
        OnInteractEnterMultiZDoor newDoor2 = door2.Generate().First().GetComponent<OnInteractEnterMultiZDoor>();

        newDoor1.Exit = newDoor2;
        newDoor2.Exit = newDoor1;
    }
}