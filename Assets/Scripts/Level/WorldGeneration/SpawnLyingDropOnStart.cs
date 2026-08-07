using System.Collections.Generic;
using UnityEngine;

public class SpawnLyingDropOnStart : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPositions = new();

    private void Start()
    {
        TryGetComponent(out Collider2D selfCollider);
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        foreach (Transform spawnPosition in _spawnPositions)
        {
            GameObject randomDrop = SpawnManager.Instance.PickRandomLyingLootDrop();
            if (randomDrop == null) continue;

            List<GameObject> result = layer.TrySpawnObject(
                randomDrop,
                spawnPosition.position,
                null,
                null
                );

            foreach (GameObject spawnedObj in result)
            {
                if (spawnedObj.TryGetComponent(out BoxCollider2D collider))
                {
                    spawnedObj.transform.position += (collider.bounds.size.y / 2f - collider.offset.y) * Vector3.up;
                }
                if (selfCollider != null && spawnedObj.TryGetComponent(out IStuckableObject stuckable))
                {
                    stuckable.StuckedToCollider = selfCollider;
                }
            }
        }
    }
}
