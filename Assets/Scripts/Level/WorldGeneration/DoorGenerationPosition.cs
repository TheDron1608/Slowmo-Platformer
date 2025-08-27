using UnityEngine;

public class DoorGenerationPosition : LateGenerateionEnviroment
{
    public OnInteractEnterMultiZDoor Door;
    public override void Generate()
    {
        OnInteractEnterMultiZDoor newDoor = Instantiate(Door, transform.position, transform.rotation, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform);
        LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForGameObject(newDoor.gameObject);
    }
    public static void GenerateDoorPair(DoorGenerationPosition door1, DoorGenerationPosition door2)
    {
        OnInteractEnterMultiZDoor newDoor1 = Instantiate(door1.Door, door1.transform.position, door1.transform.rotation, LayerManager.Instance.GetZLayerOfGameObject(door1.gameObject).transform);
        LayerManager.Instance.GetZLayerOfGameObject(newDoor1.gameObject).UpdateLayerForGameObject(newDoor1.gameObject);

        OnInteractEnterMultiZDoor newDoor2 = Instantiate(door2.Door, door2.transform.position, door2.transform.rotation, LayerManager.Instance.GetZLayerOfGameObject(door2.gameObject).transform);
        LayerManager.Instance.GetZLayerOfGameObject(newDoor2.gameObject).UpdateLayerForGameObject(newDoor2.gameObject);

        newDoor1.Exit = newDoor2;
        newDoor2.Exit = newDoor1;
    }
}