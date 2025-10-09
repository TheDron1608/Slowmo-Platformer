using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour, IStuckToObject
{
    public LootDropChanceInfo.LootSpawnerTypes LootSpawnType;
    public List<GameObject> SpawnObjectsOnBreak = new();
    public float RemoveObjectOnBreakVelocity = 3.5f;
    public float RemoveObjectOnBreakMaxRandomAngularVelocity = 360f;

    [SerializeField] protected List<ParticleSpawner> _brokenPartsParticleSpawners;

    private List<Holdable> _stuckedObjects = new();

    public event EventHandler<MonoBehaviour> OnBroken;

    public List<Holdable> StuckedObjects
    {
        get => _stuckedObjects;
    }

    public void AddStuckedObject(Holdable obj)
    {
        _stuckedObjects.Add(obj);
    }

    public void RemoveStuckedObject(Holdable obj)
    {
        _stuckedObjects.Remove(obj);
    }

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        OnBroken?.Invoke(this, breaker);

        ReleaseObjectsInside();

        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }

        RemoveAllStuckedObjects();

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

    public void RemoveAllStuckedObjects()
    {
        for (int i = 0; i < _stuckedObjects.Count; i++)
        {
            Holdable stuckObject = _stuckedObjects[i];
            if (stuckObject == null) continue;

            stuckObject.StuckedToCollider = null;
            if (stuckObject.TryGetComponent(out Rigidbody2D stuckObjectRigidBody))
            {
                stuckObjectRigidBody.linearVelocity = VectorMath.GetAngleToAsNormalizedVec2(TryGetComponent(out Collider2D collider) ? GameObjectUtility.GetCenterOfCollider(collider) : transform.position, stuckObject.transform.position) * RemoveObjectOnBreakVelocity * stuckObject.ThrowForceMultiplier;
                stuckObjectRigidBody.angularVelocity = RemoveObjectOnBreakMaxRandomAngularVelocity * (UnityEngine.Random.value * 2 - 1);
            }
        }
    }
}
