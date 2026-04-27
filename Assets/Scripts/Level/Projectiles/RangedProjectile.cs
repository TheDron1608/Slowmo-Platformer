using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class RangedProjectile : AbstractProjectile
{
    const float HOMING_MAX_ANGLE = 15f;
    const float HOMING_MAX_DISTANCE = 10f;
    const float RICOCHET_MAX_ANGLE = 10f;
    const float MAX_RANGE_RADOMIZED_EXTRA_VALUE = 1.5f;
    const float PARTICLES_ON_WALL_HIT_VELOCITY = 1f;
    const float PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY = 360f;
    const float REMOVE_PARTICLE_EFFECT_MAX_VELOCITY_MULT = 4.5f;
    const float REMOVE_PARTICLE_EFFECT_MIN_VELOCITY_MULT = 3f;
    const float REMOVE_PARTICLE_EFFECT_ANGULAR_VELOCITY = 960f;
    const float REMOVE_PARTICLE_EFFECT_ACCURACY = 0.8f;
    const float REMOVE_PARTICLE_EFFECT_DIRECTION_UP_OFFSET = 2f;
    const string PROJECTILE_TIP_GAMEOBJECT_NAME = "ProjectileTip";

    public float BulletSpeed = 35f;
    public float MaxRange = 350f;
    public float Homing = 0f;
    public PhysicsParticle BulletCasingParticle;
    public int MaxPierces = 0; //times projectiles will not doestroy iteself if gibs or cuts off damaged character
    public List<AbstractParticle> ParticlesOnWallHit = new();
    public AbstractParticle ParticleOnFaliedPierce;

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;
    private Transform _projectileTip;
    private Vector3 _positionPreviousFrame;
    private ZIndexLayer _layer;
    private int _hitLayerMask;

    private float _rangeMoved = 0f;
    private float _piercesLeft;

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

    protected override void SetAttrs(AbstractProjectile original, Quaternion direction, Vector2 position, ZIndexLayer layer, Weapon weapon)
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
        ParticleOnFaliedPierce = rangedOriginal.ParticleOnFaliedPierce;

        _rangeMoved = 0f;
        _piercesLeft = MaxPierces;

        InitEffects(original, weapon);
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
            CharacterComponentsManager bestHomingTarget = null;
            float bestHomingTargetDistance = HOMING_MAX_DISTANCE;
            foreach (Transform characterTrasnform in _layer.CharactersContainer.transform)
            {
                if (characterTrasnform.TryGetComponent(out AbstractCharacterComponent character))
                {
                    float distanceToCharacter = Vector2.Distance(ProjectileTip.position, character.CharComponents.Center.transform.position);
                    if (
                        distanceToCharacter < bestHomingTargetDistance &&
                        Vector2.Angle(transform.position, character.CharComponents.Center.transform.position) < HOMING_MAX_ANGLE &&
                        (FriendlyFire || ((!(Deflector ?? Owner)?.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam)) ?? true))
                    )
                    {
                        bestHomingTarget = character.CharComponents;
                        bestHomingTargetDistance = distanceToCharacter;
                    }
                }
            }

            if (bestHomingTarget != null)
            {
                Vector2 targetAlign = (bestHomingTarget.Center.transform.position - transform.position).normalized;
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

                OnHit(hitColliders[i].gameObject);
            }
        }

        _positionPreviousFrame = transform.position;
    }

    public override void OnHit(GameObject hitObject)
    {
        base.OnHit(hitObject);

        IDamagable damagableHitobject = hitObject.GetComponent<IDamagable>() ?? hitObject.transform.parent.GetComponent<IDamagable>();
        ObjectEffectsReceiver effectableHitobject = hitObject.GetComponent<ObjectEffectsReceiver>() ?? hitObject.transform.parent.GetComponent<ObjectEffectsReceiver>();

        if (_piercesLeft > 0 && (damagableHitobject?.PiercableThrought ?? false))
        {
            _piercesLeft--;
        }
        else if (_failedPierceThisFrame && hitObject.TryGetComponent(out Collider2D hitObjectCollider))
        {
            Vector2 ricochetDirection = (hitObjectCollider.ClosestPoint(transform.position) - VectorMath.Vec3ToVec2(transform.position)).normalized;
            if (ricochetDirection != Vector2.zero && VectorMath.GetMinAngle(MoveAlignVec2, ricochetDirection) > RICOCHET_MAX_ANGLE)
            {
                MoveAlignVec2 = -ricochetDirection;
            }
            else
            {
                RemoveProjectileWithParticleEffect(hitObjectCollider);
            }
        }
        else if (!_wasDeflectedThisFrame)
        {
            RemoveProjectile();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _currentHittingColliders.Remove(collision);
    }

    protected override bool HitCondition(List<Collider2D> totalHitObjects, Collider2D currentHitObjet)
    {
        return
            base.HitCondition(totalHitObjects, currentHitObjet) &&
            currentHitObjet.GetComponent<AbstractProjectile>() == null &&
            (!GameObjectUtility.TryGetComponentInSelfOrParentOrChild(currentHitObjet.gameObject, out IDamagable damagableHitObject) || damagableHitObject.HitableByRangedProjectiles);
    }

    public override void RemoveProjectile()
    {
        base.RemoveProjectile();
        transform.parent = ProjectilesManager.Instance.UnusedRangedProjectilesContainer;
    }

    public void RemoveProjectileWithParticleEffect(Collider2D hitTo)
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
            GetComponent<Renderer>()?.sharedMaterial,
            LayerManager.Instance.GetZLayerOfGameObject(gameObject)
            );

        RemoveProjectile();
    }
}
