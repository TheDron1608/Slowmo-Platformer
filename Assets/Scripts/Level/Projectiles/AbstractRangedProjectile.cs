using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractRangedProjectile : AbstractProjectile
{
    const float MAX_RANGE_RADOMIZED_EXTRA_VALUE = 1.5f;
    const float PARTICLES_ON_WALL_HIT_VELOCITY = 1f;
    const float PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY = 360f;
    const string PROJECTILE_TIP_GAMEOBJECT_NAME = "ProjectileTip";

    public float BulletSpeed = 35f;
    public float MaxRange = 350f;
    public PhysicsParticle BulletCasingParticle;
    public int MaxPierces = 0; //times projectiles will not doestroy iteself if gibs or cuts off damaged character

    [SerializeField] private List<AbstractParticle> _particlesOnWallHit = new();

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;
    private Transform _projectileTip;
    private Vector3 _positionPreviousFrame;
    private int _hitLayerMask;

    private float _rangeMoved = 0f;
    private float _piercesLeft;
    private bool _isFirstFrame = true;

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

    public void ResetPiercesLeft()
    {
        _piercesLeft = MaxPierces;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _positionPreviousFrame = transform.position;
        _projectileTip = transform.Find(PROJECTILE_TIP_GAMEOBJECT_NAME);

        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        _hitLayerMask = (1 << layer.CharactersLayer) | (1 << layer.EnviromentLayer) | (1 << layer.ProjectilesLayer);

        ResetPiercesLeft();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (_isFirstFrame)
        {
            _isFirstFrame = false;
            return;
        }

        RaycastHit2D[] hitObjects = Physics2D.LinecastAll(_positionPreviousFrame, _projectileTip.position, _hitLayerMask);

        Collider2D[] hitObjectsColliders = new Collider2D[hitObjects.Length];
        for (int i = 0; i < hitObjects.Length; i++)
        {
            hitObjectsColliders[i] = hitObjects[i].collider;
        }

        // invokes OnHit trigger if:
        // 1. is not hitbox of projectile's weapon's owner
        // 2. has the highest CharacterHitbox.HitPrority value than other CharacterHitboxes of the same character
        // 3. did not hit this hitbox before (resets when projectile leaves hitbox) 
        foreach (Collider2D hitObjectsCollider in hitObjectsColliders)
        {
            if (!IsAbleToHit) break;
            if (HitCondition(hitObjectsColliders, hitObjectsCollider))
            {
                _currentHittingColliders.Add(hitObjectsCollider);
                OnHit(hitObjectsCollider.gameObject);
            }
        }

        float deltaRange = BulletSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + deltaRange * _moveAlignVec2.x,
            transform.position.y + deltaRange * _moveAlignVec2.y,
            transform.position.z
            );

        _rangeMoved += deltaRange;
        if (_rangeMoved > MaxRange )
        {
            RemoveSelf();
        }
    }

    protected override void OnLateUpdate()
    {
        base.OnLateUpdate();
        _positionPreviousFrame = transform.position;
    }

    public override void OnHit(GameObject hitObject)
    {
        base.OnHit(hitObject);

        if (hitObject.tag == LayerManager.ENVIROMENT_TAG_NAME)
        {
            ParticleSpawner.SpawnParticle(
                NumberMath.PickRandomItem(_particlesOnWallHit),
                _projectileTip.transform.position,
                -VectorMath.Quartenion2DToVec2(transform.rotation),
                0f,
                PARTICLES_ON_WALL_HIT_VELOCITY,
                NumberMath.PickRandomInRangeNoSeed(-PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY, PARTICLES_ON_WALL_HIT_ANGULAR_VELOCITY),
                hitObject.TryGetComponent(out Renderer renderer) ? renderer.sharedMaterial : GetComponent<Renderer>().sharedMaterial,
                LayerManager.Instance.GetZLayerOfGameObject(gameObject)
                );
        }

        IDamagable damagableHitobject = hitObject.GetComponent<IDamagable>() ?? hitObject.transform.parent.GetComponent<IDamagable>();
        ObjectEffectsReceiver effectableHitobject = hitObject.GetComponent<ObjectEffectsReceiver>() ?? hitObject.transform.parent.GetComponent<ObjectEffectsReceiver>();
        if (
            _piercesLeft > 0 &&
            (damagableHitobject?.PiercableThrought ?? false)
            )
        {
            _piercesLeft--;
        }
        else if (!_wasDeflectedThisFrame)
        {
            RemoveSelf();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _currentHittingColliders.Remove(collision);
    }

    protected override bool HitCondition(Collider2D[] totalHitObjects, Collider2D currentHitObjet)
    {
        return 
            base.HitCondition(totalHitObjects, currentHitObjet) && 
            !currentHitObjet.TryGetComponent(out AbstractRangedProjectile rangedProjectile) &&
            (!GameObjectUtility.TryGetComponentInSelfOrParentOrChild(currentHitObjet.gameObject, out IDamagable damagableHitObject) || damagableHitObject.HitableByRangedProjectiles);
    }
}
