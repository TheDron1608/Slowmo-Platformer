using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class SpawnManagerDependedSpawner : MonoBehaviour
{
    public abstract List<GameObject> Spawn(ZIndexLayer generateWhere, Vector3Int position);
}
