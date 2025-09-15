using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkInfo
{
    public List<ChunkConnection.PreGeneratedChunkConnectionTempInfo> Connections = new();
    public List<ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo> DoorGenPositions = new();
    public BuildingInfo Building;
    public List<GameObject> ObjectsInside = new();

    public void AddObjectInside(GameObject obj)
    {
        ObjectsInside.Add(obj);

        if (obj.TryGetComponent(out BreakableObject breakableObj))
        {
            breakableObj.OnBroken += BreakableObj_OnBroken;
        }
    }

    private void BreakableObj_OnBroken(object sender, MonoBehaviour e)
    {
        ObjectsInside.Remove(((MonoBehaviour)sender).gameObject);
    }

    ~ChunkInfo()
    {
        foreach (GameObject objectInside in ObjectsInside)
        {
            if (objectInside?.TryGetComponent(out BreakableObject breakableObj) ?? false)
            {
                breakableObj.OnBroken -= BreakableObj_OnBroken;
            }
        }
    }
}
