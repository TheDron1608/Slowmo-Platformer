using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(1)]
public class RangedProjectile : AbstractProjectile
{
    const float HOMING_TARGET_UPDATE_PER_SECOND = 2f;
    //const float RICOCHET_MIN_ANGLE = 60f;
    const float PARTICLES_ON_WALL_HIT_VELOCITY = 1f;
    const float PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY = 360f;
    const float REMOVE_PARTICLE_EFFECT_MAX_VELOCITY_MULT = 4.5f;
    const float REMOVE_PARTICLE_EFFECT_MIN_VELOCITY_MULT = 3f;
    const float REMOVE_PARTICLE_EFFECT_ANGULAR_VELOCITY = 960f;
    const float REMOVE_PARTICLE_EFFECT_ACCURACY = 0.8f;
    const float REMOVE_PARTICLE_EFFECT_DIRECTION_UP_OFFSET = 2f;
    const float MIN_DISTANCE_FROM_SPAWN_POSITION_TO_CREATE_STUCK_PARTICLE = 1.5f;
    const string PROJECTILE_TIP_GAMEOBJECT_NAME = "ProjectileTip";
    const float DISTANCE_TO_PLAYER_WHOOSH_SOUND = 4.5f;

    const float HIT_PARTICLES_MIN_SPAWN_VELOCITY = 1f;
    const float HIT_PARTICLES_MAX_SPAWN_VELOCITY = 4f;
    const float HIT_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY = -180f;
    const float HIT_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY = 180f;

    public float BulletSpeed = 35f;
    public float MaxRange = 350f;
    public float Homing = 0f;
    public float ShotNoiseDistance = 7.5f;
    public PhysicsParticle BulletCasingParticle;
    public int MaxPierces = 0; //times projectiles will not doestroy iteself if gibs or cuts off damaged character
    public List<AbstractParticle> ParticlesOnWallHit = new();
    public List<AbstractParticle> InstantParticlesOnAnyHit = new();
    public AbstractParticle ParticleOnFaliedPierce;
    public PhysicsParticle ParticleOnHit;
    public bool PierceWalls = false;
    public bool PierceObjects = true;
    public SoundPlayer WhooshSoundPlayer;

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;
    private Transform _projectileTip;
    private Vector3 _positionPreviousFrame;
    private ZIndexLayer _layer;
    private int _hitLayerMask;
    private Vector2 _spawnPosition;

    private float _rangeMoved = 0f;
    private float _piercesLeft;
    private float _timeSinceHomingTargetUpdate = 0f;
    private AbstractCharacterComponent _currentHomingTarget = null;

    public Quaternion MoveAlign
    {
        get => _moveAlign;
        set
        {
            transform.rotation = value;
            _moveAlign = value.normalized;
            _moveAlignVec2 = VectorMath.Quartenion2DToVec3(_moveAlign);
        }
    }

    public Vector2 MoveAlignVec2
    {
        get => _moveAlignVec2;
        set
        {
            _moveAlignVec2 = value.normalized;
            _moveAlign = VectorMath.Vec2ToQuarterninon2D(_moveAlignVec2);
            transform.rotation = MoveAlign;
        }
    }

    public Transform ProjectileTip
    {
        get => _projectileTip;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        _projectileTip = transform.Find(PROJECTILE_TIP_GAMEOBJECT_NAME);
    }

    protected override void SetAttrs(AbstractProjectile original, Quaternion direction, Vector2 position, ZIndexLayer layer, MonoBehaviour weapon)
    {
        base.SetAttrs(original, direction, position, layer, weapon);

        _positionPreviousFrame = transform.position;
        _layer = layer;
        _hitLayerMask = (1 << layer.CharactersLayer) | (1 << layer.EnviromentLayer) | (1 << layer.ProjectilesLayer) | (1 << layer.HitableHoldablesLayer);
        MoveAlign = direction;

        RangedProjectile rangedOriginal = original.GetComponent<RangedProjectile>();
        BulletSpeed = rangedOriginal.BulletSpeed;
        Homing = rangedOriginal.Homing;
        MaxRange = rangedOriginal.MaxRange;
        BulletCasingParticle = rangedOriginal.BulletCasingParticle;
        MaxPierces = rangedOriginal.MaxPierces;
        ParticlesOnWallHit = rangedOriginal.ParticlesOnWallHit;
        InstantParticlesOnAnyHit = rangedOriginal.InstantParticlesOnAnyHit;
        ParticleOnFaliedPierce = rangedOriginal.ParticleOnFaliedPierce;
        ShotNoiseDistance = rangedOriginal.ShotNoiseDistance;
        ParticleOnHit = rangedOriginal.ParticleOnHit;
        PierceWalls = rangedOriginal.PierceWalls;
        PierceObjects = rangedOriginal.PierceObjects;

        WhooshSoundPlayer.DefaultSound = rangedOriginal.WhooshSoundPlayer.DefaultSound;
        WhooshSoundPlayer.Volume = rangedOriginal.WhooshSoundPlayer.Volume;
        WhooshSoundPlayer.Pitch = rangedOriginal.WhooshSoundPlayer.Pitch;

        _rangeMoved = 0f;
        _piercesLeft = MaxPierces;
        _timeSinceHomingTargetUpdate = 0f;
        _currentHomingTarget = null;
        _spawnPosition = position;

        InitEffects(original, weapon);

        UpdateHomingTarget();
        CommitNoise();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        float deltaRange = BulletSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + deltaRange * _moveAlignVec2.x,
            transform.position.y + deltaRange * _moveAlignVec2.y,
            transform.position.z
            );

        _rangeMoved += deltaRange;
        if (_rangeMoved > MaxRange)
        {
            RemoveProjectile();
        }
    }

    private void FixedUpdate()
    {
        //update homing
        if (Homing != 0f)
        {
            if (_timeSinceHomingTargetUpdate > 1f / HOMING_TARGET_UPDATE_PER_SECOND)
            {
                _timeSinceHomingTargetUpdate = 0f;
                UpdateHomingTarget();
            }
            else
            {
                _timeSinceHomingTargetUpdate += Time.deltaTime;
            }

            if (_currentHomingTarget != null)
            {
                Vector2 targetAlign = (_currentHomingTarget.CharComponents.Center.transform.position - transform.position).normalized;
                MoveAlignVec2 = new Vector2(
                    math.lerp(MoveAlignVec2.x, targetAlign.x, Homing * Time.fixedDeltaTime),
                    math.lerp(MoveAlignVec2.y, targetAlign.y, Homing * Time.fixedDeltaTime)
                    );
            }
        }

        //hit enemies
        RaycastHit2D[] hits = Physics2D.LinecastAll(_positionPreviousFrame, _projectileTip.position, _hitLayerMask);
        List<Collider2D> hitColliders = new List<Collider2D>(hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            hitColliders.Insert(i, hits[i].collider);
        }

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        for (int i = 0; i < hits.Length; i++)
        {
            if (!IsAbleToHit) break;
            if (HitCondition(hitColliders, hitColliders[i]))
            {
                _currentHittingColliders.Add(hitColliders[i]);

                if (hitColliders[i].tag == LayerManager.ENVIROMENT_TAG_NAME)
                {
                    ParticleSpawner.SpawnParticle(
                        NumberMath.PickRandomItem(ParticlesOnWallHit),
                        hits[i].point,
                        -VectorMath.Quartenion2DToVec2(transform.rotation),
                        0f,
                        PARTICLES_ON_WALL_HIT_VELOCITY,
                        NumberMath.PickRandomInRangeNoSeed(-PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY, PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY),
                        hitColliders[i].TryGetComponent(out Renderer renderer) ? renderer.sharedMaterial : GetComponent<Renderer>().sharedMaterial,
                        _layer
                        );
                }

                if (InstantParticlesOnAnyHit.Count > 0)
                {
                    ParticleSpawner.SpawnInstantlyMultipleParticles(
                        InstantParticlesOnAnyHit,
                        hits[i].point,
                        -VectorMath.Quartenion2DToVec2(transform.rotation),
                        0f,
                        HIT_PARTICLES_MIN_SPAWN_VELOCITY,
                        HIT_PARTICLES_MAX_SPAWN_VELOCITY,
                        HIT_PARTICLES_MIN_SPAWN_ANGULAR_VELOCITY,
                        HIT_PARTICLES_MAX_SPAWN_ANGULAR_VELOCITY,
                        hitColliders[i].TryGetComponent(out Renderer renderer2) ? renderer2.sharedMaterial : GetComponent<Renderer>().sharedMaterial,
                        _layer,
                        InstantParticlesOnAnyHit.Count - 1,
                        0f,
                        true,
                        false
                        );
                }

                OnHit(hitColliders[i].gameObject);
            }
        }

        if (
            Owner != null &&
            !Owner.IsDestroyed() &&
            !WhooshSoundPlayer.GetIsPlaying() &&
            Vector2.Distance(Camera.main.transform.position, ProjectileTip.transform.position) < DISTANCE_TO_PLAYER_WHOOSH_SOUND &&
            !Camera.main.GetComponent<CameraTrack>().TrackTargets.Contains(Owner.transform)
            )
        {
            WhooshSoundPlayer.PlaySound();
        }

        _positionPreviousFrame = transform.position;
    }

    private void UpdateHomingTarget()
    {
        if (_currentHomingTarget != null) return;

        int raycastMask = 1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EnviromentLayer;
        AbstractCharacterComponent bestHomingTarget = null;
        float bestHomingAngle = float.MaxValue;
        foreach (Transform characterTransform in _layer.CharactersContainer.transform)
        {
            if (
                characterTransform.gameObject.activeInHierarchy &&
                characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
                )
            {
                float currentHomingAngle = Vector2.Angle(MoveAlignVec2, character.CharComponents.Center.transform.position - transform.position);
                if (
                    bestHomingAngle > currentHomingAngle &&
                    (FriendlyFire || ((!(Deflector ?? Owner)?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam)) ?? true)) &&
                    Physics2D.Linecast(ProjectileTip.transform.position, character.CharComponents.Center.transform.position, raycastMask).collider == null
                )
                {
                    bestHomingTarget = character;
                    bestHomingAngle = currentHomingAngle;
                }
            }
        }

        _currentHomingTarget = bestHomingTarget;
    }

    public void CommitNoise()
    {
        NoiseManager.Instance.CommitNoise(
            transform.position,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject),
            ShotNoiseDistance * ((Weapon?.TryGetComponent(out Weapon weaponComponent) ?? false) ? weaponComponent.AttackNoiseMultiplier : 1f),
            gameObject,
            (Deflector ?? Owner)?.CharComponents.CharacterTeam
            );
    }

    public override void OnHit(GameObject hitObject)
    {
        base.OnHit(hitObject);

        GameObjectUtility.TryGetComponentInSelfOrParent(hitObject, out IDamagable damagableHitobject);
        GameObjectUtility.TryGetComponentInSelfOrParent(hitObject, out ObjectEffectsReceiver effectableHitobject);
        hitObject.TryGetComponent(out Collider2D hitObjectCollider);

        if (_failedPierceThisFrame && hitObjectCollider != null)
        {
            /*Vector2 ricochetDirection = (hitObjectCollider.ClosestPoint(transform.position) - VectorMath.Vec3ToVec2(transform.position)).normalized;
            if (ricochetDirection != Vector2.zero && VectorMath.GetMinAngle(MoveAlignVec2, ricochetDirection) > RICOCHET_MIN_ANGLE)
            {
                MoveAlignVec2 = -ricochetDirection;
            }
            else
            {
                RemoveProjectileWithParticleEffect(hitObjectCollider);
            }*/
            FailedPierceParticleEffect(hitObjectCollider);

            RemoveProjectile();
        }

        if (damagableHitobject?.AlwaysPierce ?? false)
        {

        }
        else if (_piercesLeft > 0 && PierceWalls && hitObject.TryGetComponent(out Tilemap tilemap))
        {
            if (_piercesLeft > 0)
            {
                _layer.MultiTileMapsContainer.DestroyTileAt(new Vector3Int((int)math.floor(transform.position.x), (int)math.floor(transform.position.y), 0), false, false);
                _piercesLeft--;
            }
            if (_piercesLeft > 0)
            {
                _layer.MultiTileMapsContainer.DestroyTileAt(new Vector3Int((int)math.floor(_projectileTip.transform.position.x), (int)math.floor(_projectileTip.transform.position.y), 0), false, false);
                _piercesLeft--;
            }
        }
        else if (_piercesLeft > 0 && (damagableHitobject?.PiercableThrought ?? false) && PierceObjects)
        {
            _piercesLeft--;
        }
        else if (!_wasDeflectedThisFrame)
        {
            if (ParticleOnHit != null && hitObjectCollider != null)
            {
                StuckParticleEffect(hitObjectCollider);
            }

            RemoveProjectile();
        }
    }

    public override void OnDeflected(MonoBehaviour deflector)
    {
        base.OnDeflected(deflector);

        _currentHomingTarget = null;
        UpdateHomingTarget();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _currentHittingColliders.Remove(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (PierceWalls)
        {
            Vector3Int targetPos1 = new Vector3Int(
                (int)math.floor(transform.position.x), 
                (int)math.floor(transform.position.y), 
                0
                );
            Vector3Int targetPos2 = new Vector3Int(
                (int)math.floor((_projectileTip.transform.position.x + transform.position.x) / 2f), 
                (int)math.floor((_projectileTip.transform.position.y + transform.position.y) / 2f), 
                0
                );
            Vector3Int targetPos3 = new Vector3Int(
                (int)math.floor(_projectileTip.transform.position.x), 
                (int)math.floor(_projectileTip.transform.position.y), 
                0
                );

            if (_layer.MultiTileMapsContainer.GetHasValidAsPlatformAt(targetPos1))
            {
                _layer.MultiTileMapsContainer.DestroyTileAt(targetPos1, false, false);
                _piercesLeft--;
            }
            if (_layer.MultiTileMapsContainer.GetHasValidAsPlatformAt(targetPos2))
            {
                _layer.MultiTileMapsContainer.DestroyTileAt(targetPos2, false, false);
                _piercesLeft--;
            }
            if (_layer.MultiTileMapsContainer.GetHasValidAsPlatformAt(targetPos3))
            {
                _layer.MultiTileMapsContainer.DestroyTileAt(targetPos3, false, false);
                _piercesLeft--;
            }

            if (_piercesLeft < 0)
            {
                RemoveProjectile();
            }
        }
    }

    protected override bool HitCondition(List<Collider2D> totalHitObjects, Collider2D currentHitObjet)
    {
        return
            base.HitCondition(totalHitObjects, currentHitObjet) &&
            !currentHitObjet.TryGetComponent(out AbstractProjectile ah) &&
            (!GameObjectUtility.TryGetComponentInSelfOrParentOrChild(currentHitObjet.gameObject, out IDamagable damagableHitObject) || damagableHitObject.HitableByRangedProjectiles);
    }

    public override void RemoveProjectile()
    {
        base.RemoveProjectile();
        transform.parent = ProjectilesManager.Instance.UnusedRangedProjectilesContainer;
    }

    public void FailedPierceParticleEffect(Collider2D hitTo)
    {
        ParticleSpawner.SpawnParticle(
            ParticleOnFaliedPierce,
            hitTo.ClosestPoint(transform.position),
            VectorMath.RandomizeVec2(
                (transform.position - hitTo.bounds.center).normalized + Vector3.up * REMOVE_PARTICLE_EFFECT_DIRECTION_UP_OFFSET, 
                REMOVE_PARTICLE_EFFECT_ACCURACY
                ),
            0f,
            NumberMath.PickRandomInRangeNoSeed(REMOVE_PARTICLE_EFFECT_MIN_VELOCITY_MULT, REMOVE_PARTICLE_EFFECT_MAX_VELOCITY_MULT),
            NumberMath.PickRandomInRangeNoSeed(-REMOVE_PARTICLE_EFFECT_ANGULAR_VELOCITY, REMOVE_PARTICLE_EFFECT_ANGULAR_VELOCITY),
            TryGetComponent(out Renderer renderer) ? renderer.sharedMaterial : null,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject)
            );
    }

    public void StuckParticleEffect(Collider2D hitTo)
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        if (layer == null) return;

        if (hitTo is TilemapCollider2D && Vector2.Distance(_spawnPosition, transform.position) < MIN_DISTANCE_FROM_SPAWN_POSITION_TO_CREATE_STUCK_PARTICLE) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, MoveAlignVec2, 5f, (1 << layer.EnviromentLayer) | (1 << layer.CharactersLayer) | (1 << layer.HitableHoldablesLayer));
        if (hit.collider == null) return;

        PhysicsParticle newParticle = ParticleSpawner.SpawnParticle(
            ParticleOnHit,
            hit.point,
            MoveAlignVec2,
            MoveAlign.eulerAngles.z,
            NumberMath.PickRandomInRangeNoSeed(REMOVE_PARTICLE_EFFECT_MIN_VELOCITY_MULT, REMOVE_PARTICLE_EFFECT_MAX_VELOCITY_MULT),
            NumberMath.PickRandomInRangeNoSeed(-REMOVE_PARTICLE_EFFECT_ANGULAR_VELOCITY, REMOVE_PARTICLE_EFFECT_ANGULAR_VELOCITY),
            TryGetComponent(out Renderer renderer) ? renderer.sharedMaterial : null,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject),
            false
            ) as PhysicsParticle;

        if (newParticle != null)
        {
            newParticle.StuckedToCollider = hit.collider;
        }
    }
}
