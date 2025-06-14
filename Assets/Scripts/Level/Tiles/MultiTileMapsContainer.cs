using UnityEngine;
using UnityEngine.Tilemaps;

public class MultiTileMapsContainer : MonoBehaviour
{
    public Tilemap GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType behaviourType)
    {
        foreach (TileBehaviour tileBehaviour in transform.GetComponentsInChildren<TileBehaviour>())
        {
            if (tileBehaviour.BehaviourType == behaviourType) return tileBehaviour.GetComponent<Tilemap>();
        }
        return null;
    }

    public Tilemap[] GetTileMaps()
    {
        return transform.GetComponentsInChildren<Tilemap>();
    }
}
