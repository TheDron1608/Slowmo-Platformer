using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreakableObject : MonoBehaviour, IStuckToObject
{
    const float BREAK_PARTICLES_ACCURACY = 0f;
    const float BREAK_PARTICLES_MIN_SPAWN_VELOCITY = 1f;
    const float BREAK_PARTICLES_MAX_SPAWN_VELOCITY = 4f;
    const float BREAK_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY = -180f;
    const float BREAK_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY = 180f;
    const float BREAK_DIRECTIVE_PARTICLES_ACCURACY = 0.4f;

    public LootDropChanceInfo.LootSpawnerTypes LootSpawnType;
    public List<GameObject> SpawnObjectsOnBreak = new();
    public float RemoveObjectOnBreakVelocity = 3.5f;
    public float RemoveObjectOnBreakMaxRandomAngularVelocity = 360f;
    public List<AbstractEffect> SelfEffectsOnBreak = new();

    [SerializeField] private List<AbstractParticle> _partcilesOnBreak;
    public AbstractSoundPlayer SoundOnBreak;

    private List<IStuckableObject> _stuckedObjects = new();
    private bool _isBreakingThisFrame = false;

    public event EventHandler<MonoBehaviour> OnBroken;

    public List<IStuckableObject> StuckedObjects
    {
        get => _stuckedObjects;
    }

    public List<AbstractParticle> ParticlesOnBreak
    {
        get => _partcilesOnBreak;
        set => _partcilesOnBreak = value;
    }

    public virtual void AddStuckedObject(IStuckableObject obj)
    {
        _stuckedObjects.Add(obj);
        StuckTrackTarget.CreateTrack(obj, transform);
    }

    public virtual void RemoveStuckedObject(IStuckableObject obj)
    {
        _stuckedObjects.Remove(obj);
    }

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        ReleaseObjectsInsideAndApplyEffects();

        BreakObjectVisualOnly(breaker);

        Destroy(gameObject);
    }
    protected virtual void BreakObjectVisualOnly(MonoBehaviour breaker)
    {
        OnBroken?.Invoke(this, breaker);

        SpawnBrokenParticlesAndPlaySound(breaker);

        RemoveAllStuckedObjects();
    }

    public void ReleaseObjectsInsideAndApplyEffects()
    {
        if (_isBreakingThisFrame == true) return;
        _isBreakingThisFrame = true;

        if (TryGetComponent(out ObjectEffectsReceiver effectsReceiver))
        {
            effectsReceiver.ApplyEffect(SelfEffectsOnBreak, this);
        }

        ReleaseObjectInside();
    }

    public void ReleaseObjectInside()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        Vector2 spawnPosition = TryGetComponent(out Collider2D collider) ? GameObjectUtility.GetCenterOfCollider(collider) : transform.position;

        foreach (GameObject objectInside in SpawnObjectsOnBreak)
        {
            GameObject particleGO = layer.TrySpawnObject(
                 objectInside.gameObject,
                 spawnPosition,
                 null,
                 null
                 ).FirstOrDefault();

            if (TryGetComponent(out Renderer renderer) && particleGO != null && particleGO.TryGetComponent(out Renderer particleRenderer))
            {
                particleRenderer.sharedMaterial = renderer.sharedMaterial;
            }
        }
        SpawnObjectsOnBreak.Clear();

        foreach (GameObject objectInsideGlobal in SpawnManager.Instance.GetLootDropsByType(LootSpawnType))
        {
            layer.TrySpawnObject(
                objectInsideGlobal.gameObject,
                spawnPosition,
                null,
                null
                );
        }
    }

    public void RemoveAllStuckedObjects()
    {
        for (int i = 0; i < _stuckedObjects.Count; i++)
        {
            IStuckableObject stuckObject = _stuckedObjects[i];
            if (stuckObject == null) continue;

            stuckObject.StuckedToCollider = null;
            if ((stuckObject as MonoBehaviour).TryGetComponent(out Rigidbody2D stuckObjectRigidBody))
            {
                (stuckObject as MonoBehaviour).TryGetComponent(out Holdable stuckedHoldable);
                stuckObjectRigidBody.linearVelocity = VectorMath.GetAngleToAsNormalizedVec2(TryGetComponent(out Collider2D collider) ? 
                    GameObjectUtility.GetCenterOfCollider(collider) : 
                    transform.position, (stuckObject as MonoBehaviour).transform.position) * RemoveObjectOnBreakVelocity * (stuckedHoldable?.ThrowForceMultiplier ?? 1f);
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
            TryGetComponent(out Collider2D collider) ? GameObjectUtility.GetCenterOfCollider(collider) : transform.position,
            VectorMath.Quartenion2DToVec2(breaker?.transform.rotation ?? transform.rotation),
            NumberMath.PickRandomInRangeNoSeed(0f, 360f),
            BREAK_PARTICLES_MIN_SPAWN_VELOCITY,
            BREAK_PARTICLES_MAX_SPAWN_VELOCITY,
            BREAK_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
            BREAK_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
            TryGetComponent(out Renderer renderer) ? renderer.sharedMaterial : null,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject),
            _partcilesOnBreak.Count,
            BREAK_DIRECTIVE_PARTICLES_ACCURACY,
            true,
            false
            );
    }
}
