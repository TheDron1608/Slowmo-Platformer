using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomSpawn : MonoBehaviour
{
    public float GenerateChance = 1f;

    public GameObject PickRandomSpawnObject()
    {
        if (GenerateChance < 1f && Random.value > GenerateChance) return null;
        return transform.GetChild(Mathf.RoundToInt(Random.value * (transform.childCount - 1))).gameObject;
    }
}
