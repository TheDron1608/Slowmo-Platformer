using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreakableObject : MonoBehaviour, IStuckToObject
{
    const float BREAK_PARTICLES_ACCURACY = 0.85f;
    const float BREAK_PARTICLES_MIN_SPAWN_VELOCITY = 1f;
    const float BREAK_PARTICLES_MAX_SPAWN_VELOCITY = 4f;
    const float BREAK_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY = -180f;
    const float BREAK_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY = 180f;
    const float BREAK_DIRECTIVE_PARTICLES_ACCURACY = 0.4f;

    public LootDropChanceInfo.LootSpawnerTypes LootSpawnType;
    public List<GameObject> SpawnObjectsOnBreak = new();
    public float RemoveObjectOnBreakVelocity = 3.5f;
    public float RemoveObjectOnBreakMaxRandomAngularVelocity = 360f;

    [SerializeField] private List<AbstractParticle> _partcilesOnBreak;
    public AbstractSoundPlayer SoundOnBreak;

    private List<Holdable> _stuckedObjects = new();

    public event EventHandler<MonoBehaviour> OnBroken;

    public List<Holdable> StuckedObjects
    {
        get => _stuckedObjects;
    }

    public virtual void AddStuckedObject(Holdable obj)
    {
        _stuckedObjects.Add(obj);
        StuckTrackTarget.CreateTrack(obj, transform);
    }

    public virtual void RemoveStuckedObject(Holdable obj)
    {
        _stuckedObjects.Remove(obj);
    }

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        OnBroken?.Invoke(this, breaker);

        ReleaseObjectsInside();

        SpawnBrokenParticlesAndPlaySound(breaker);

        RemoveAllStuckedObjects();

        Destroy(gameObject);
    }

    public void ReleaseObjectsInside()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        Vector2 spawnPosition = TryGetComponent(out Collider2D collider) ? GameObjectUtility.GetCenterOfCollider(collider) : transform.position;

        foreach (GameObject objectInside in SpawnObjectsOnBreak)
        {
            layer.TrySpawnObject(
                objectInside.gameObject,
                NumberMath.Vec3ToVec3Int(spawnPosition),
                null,
                null
                );
        }
        SpawnObjectsOnBreak.Clear();

        foreach (GameObject objectInsideGlobal in SpawnManager.Instance.GetLootDropsByType(LootSpawnType))
        {
            layer.TrySpawnObject(
                objectInsideGlobal.gameObject,
                NumberMath.Vec3ToVec3Int(spawnPosition),
                null,
                null
                );
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
        StuckedObjects.Clear();
    }

    protected void SpawnBrokenParticlesAndPlaySound(MonoBehaviour breaker)
    {
        SoundOnBreak.PlaySound(false, transform.position);

        if (_partcilesOnBreak.Count == 0) return;

        ParticleSpawner.SpawnInstantlyMultipleParticles(
            _partcilesOnBreak,
            GameObjectUtility.GetCenterOfCollider(GetComponent<Collider2D>()),
            VectorMath.Quartenion2DToVec2(breaker.transform.rotation),
            NumberMath.PickRandomInRangeNoSeed(0f, 360f),
            BREAK_PARTICLES_MIN_SPAWN_VELOCITY,
            BREAK_PARTICLES_MAX_SPAWN_VELOCITY,
            BREAK_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
            BREAK_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
            GetComponent<Renderer>()?.sharedMaterial,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject),
            _partcilesOnBreak.Count,
            BREAK_DIRECTIVE_PARTICLES_ACCURACY
            );
    }
}
