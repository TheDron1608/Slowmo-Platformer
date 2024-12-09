using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public class OnCollisionChangedEventArgs
    {
        public OnCollisionChangedEventArgs(bool enterOrReleasedCollision, Vector2 collisionAlign)
        {
            EnterOrReleasedCollision = enterOrReleasedCollision;
            CollisionAlign = collisionAlign;
        }

        bool EnterOrReleasedCollision;
        public Vector2 CollisionAlign;
    }

    const float COLLISION_HIT_DETECION_THICKNESS = 0.01f;
    const float COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET = 0.7f; //value between 0 and 1
    const string COLLISION_HIT_DETECTION_LAYER_NAME = "EnviromentColliders";

    public event EventHandler<OnCollisionChangedEventArgs> OnCollisionChanged;

    private Rigidbody2D _rigidBodyComponent;
    private CapsuleCollider2D _capsuleColliderComponent;

    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;
    private int _collisionDetectionLayerMask;

    private bool _isCollidingFloor = false;
    private bool _isCollidingRoof = false;
    private bool _isCollidingLeftWall = false;
    private bool _isCollidingRightWall = false;


    public float TimeInAir
    {
        get => _timeInAir;
        private set => _timeInAir = value;
    }
    public float TimeOnGround
    {
        get => _timeOnGround;
        private set => _timeOnGround = value;
    }

    private bool IsCollidingFrom(Vector2 from, Vector2 align)
    {
        float rayCastHitRange = (align.x != 0 ? _capsuleColliderComponent.size.x : _capsuleColliderComponent.size.y) / 2 + COLLISION_HIT_DETECION_THICKNESS;
        RaycastHit2D rayCastHit = Physics2D.Raycast(from, align, rayCastHitRange, _collisionDetectionLayerMask);
        return rayCastHit.collider != null;
    }
        
    private bool IsCollidingFromCenter(Vector2 align)
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y) + _capsuleColliderComponent.offset;
        return IsCollidingFrom(rayCastHitOrigin, align);
    }

    private bool IsCollidingFromHead(Vector2 align)
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y + (_capsuleColliderComponent.size.y - _capsuleColliderComponent.size.x * COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET) / 2) + _capsuleColliderComponent.offset;
        return IsCollidingFrom(rayCastHitOrigin, align);
    }

    private bool IsCollidingFromLegs(Vector2 align)
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y - (_capsuleColliderComponent.size.y - _capsuleColliderComponent.size.x * COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET) / 2) + _capsuleColliderComponent.offset;
        return IsCollidingFrom(rayCastHitOrigin, align);
    }

    public bool IsCollidingFloor()
    {
        return IsCollidingFromCenter(Vector2.down);
    }

    public bool IsCollidingRoof()
    {
        return IsCollidingFromCenter(Vector2.up);
    }

    public bool IsCollidingLeftWall()
    {
        return (
            IsCollidingFromCenter(Vector2.left) ||
            IsCollidingFromHead(Vector2.left) ||
            IsCollidingFromLegs(Vector2.left)
            );


    }

    public bool IsCollidingRightWall()
    {
        return (
            IsCollidingFromCenter(Vector2.right) ||
            IsCollidingFromHead(Vector2.right) ||
            IsCollidingFromLegs(Vector2.right)
            );
    }



    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CapsuleCollider2D>(out _capsuleColliderComponent)) throw new UnityException("CapsuleCollider2D component not found");
        _collisionDetectionLayerMask = 1 << LayerMask.NameToLayer(COLLISION_HIT_DETECTION_LAYER_NAME);
    }

    private void Update()
    {
        UpdateTimeOnAirOrGround();
    }

    private void UpdateTimeOnAirOrGround()
    {
        if (IsCollidingFloor())
        {
            _timeOnGround += Time.deltaTime;
            _timeInAir = 0f;
        }
        else
        {
            _timeInAir += Time.deltaTime;
            _timeOnGround = 0f;
        }
    }

    private void UpdateCollidingInfo()
    {
        bool wasCollidingFloor = _isCollidingFloor;
        bool wasCollidingRoof = _isCollidingRoof;
        bool wasCollidingLeftWall = _isCollidingLeftWall;
        bool wasCollidingRightWall = _isCollidingRightWall;

        _isCollidingFloor = IsCollidingFloor();
        _isCollidingRoof = IsCollidingRoof();
        _isCollidingLeftWall = IsCollidingLeftWall();
        _isCollidingRightWall = IsCollidingRightWall();

        if (wasCollidingFloor != _isCollidingFloor) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingFloor, Vector2.down));
        if (wasCollidingRoof != _isCollidingRoof) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingRoof, Vector2.up));
        if (wasCollidingLeftWall != _isCollidingLeftWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingLeftWall, Vector2.left));
        if (wasCollidingRightWall != _isCollidingRightWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingRightWall, Vector2.right));
    }



    private void LateUpdate()
    {
        _wasGroundedPrevFrame = IsCollidingFloor();
    }
}
