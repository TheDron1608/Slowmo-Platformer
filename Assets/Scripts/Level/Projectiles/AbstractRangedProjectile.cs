using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractRangedProjectile : AbstractSingleProjectile
{
    const float MAX_RANGE_RADOMIZED_EXTRA_VALUE = 1.5f;

    public float BulletSpeed = 35f;
    public float MaxRange = 350f;

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;

    private float _rangeMoved = 0f;
    private bool _isFirstFrame = true;

    public Quaternion MoveAlign
    {
        get => _moveAlign;
        set
        {
            transform.rotation = value;
            _moveAlign = value.normalized;
            _moveAlignVec2 = VectorMath.Quartenion2DToVec2(_moveAlign);
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

    public override AbstractProjectile SpawnProjectile(Quaternion direction, float accuracityMultiplier = 1f, Weapon weapon = null)
    {
        base.SpawnProjectile(direction, accuracityMultiplier, weapon);

        MoveAlign = direction;

        return null;
    }

    private void Update()
    {
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
            Remove();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (
            Weapon != null &&
            collision.gameObject != Weapon.gameObject && 
            Weapon.TryGetComponent(out Holdable weaponHoldableComponent) &&
            (weaponHoldableComponent.CurrentHolder == null || collision.gameObject != weaponHoldableComponent.CurrentHolder.gameObject)
            )
        {
            Remove();
        }

    }
}
