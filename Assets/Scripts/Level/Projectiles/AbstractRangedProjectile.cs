using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractRangedProjectile : AbstractProjectile
{
    const float MAX_RANGE_RADOMIZED_EXTRA_VALUE = 1.5f;
    const string ON_HIT_WALL_CLOUDS_PARTICLE_GAMEOBJECT_NAME = "OnHitWallCloudParticle";

    public float BulletSpeed = 35f;
    public float MaxRange = 350f;
    public PhysicsParticle BulletCasingParticle;
    public int MaxPierces = 0; //times projectiles will not doestroy iteself if gibs or cuts off damaged character

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;
    private ParticleSpawner _onHitWallCloudsPaticleSpawner;

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

    public void ResetPiercesLeft()
    {
        _piercesLeft = MaxPierces;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        _onHitWallCloudsPaticleSpawner = transform.Find(ON_HIT_WALL_CLOUDS_PARTICLE_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
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

    public override void OnHit(GameObject hitObject)
    {
        base.OnHit(hitObject);

        if (hitObject.tag == LayerManager.ENVIROMENT_TAG_NAME)
        {
            _onHitWallCloudsPaticleSpawner.SpawnParticle();
        }

        if (
            _piercesLeft > 0 &&
            hitObject.transform.parent.TryGetComponent(out AbstractCharacterComponent hitCharacter) &&
            hitCharacter.CharComponents.CharacterEffects.GetHasEffect<Death>()
            )
        {
            _piercesLeft--;
        }
        else
        {
            RemoveSelf();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _currentHittingColliders.Remove(collision);
    }

    protected override bool HitCondition(List<Collider2D> totalHitObjects, Collider2D currentHitObjet)
    {
        return base.HitCondition(totalHitObjects, currentHitObjet) && !currentHitObjet.TryGetComponent(out AbstractRangedProjectile rangedProjectile);
    }
}
