using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public LootDropChanceInfo.LootSpawnerTypes LootSpawnType;
    public List<GameObject> SpawnObjectsOnBreak = new();

    [SerializeField] protected List<ParticleSpawner> _brokenPartsParticleSpawners;

    public event EventHandler<MonoBehaviour> OnBroken;

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        OnBroken?.Invoke(this, breaker);

        ReleaseObjectsInside();

        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }

        Destroy(gameObject);
    }

    public void ReleaseObjectsInside()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        Vector2 spawnPosition = TryGetComponent(out Collider2D collider) ? GameObjectUtility.GetCenterOfCollider(collider) : transform.position;

        foreach (GameObject objectInside in SpawnObjectsOnBreak)
        {
            GameObject newObject = Instantiate(objectInside, spawnPosition, objectInside.transform.rotation, transform);
            LayerManager.Instance.ChangeZIndexForGameObject(layer, newObject);
        }
        SpawnObjectsOnBreak.Clear();

        foreach (GameObject objectInsideGlobal in SpawnManager.Instance.GetLootDropsByType(LootSpawnType))
        {
            GameObject newObject = Instantiate(objectInsideGlobal, spawnPosition, objectInsideGlobal.transform.rotation, transform);
            LayerManager.Instance.ChangeZIndexForGameObject(layer, newObject);
        }
    }
}
