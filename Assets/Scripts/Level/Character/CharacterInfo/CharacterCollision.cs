using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterCollision : AbstractCharacterComponent
{
    const string ENVIROMENT_TAG_NAME = "Enviroment";

    public class OnCollisionChangedEventArgs
    {
        public OnCollisionChangedEventArgs(bool enterOrReleasedCollision, Vector2 collisionAlign, GameObject collider)
        {
            EnterOrReleasedCollision = enterOrReleasedCollision;
            CollisionAlign = collisionAlign;
            Collider = collider;
        }

        /// <summary>
        /// If true enteres collision, else releases collision
        /// </summary>
        public bool EnterOrReleasedCollision;
        public Vector2 CollisionAlign;
        public GameObject Collider;
    }

    public float SpeedToHitOtherCharacters = 7.5f;
    public bool CanHitWhileHardStnned = true;
    public bool CanHitWhileMoving = false;
    public bool CanHitWhileRolling = false;
    public List<AbstractEffect> EffectsOnHitOtherCharacters = new();
    public List<AbstractEffect> SelfEffectsOnHitOtherCharacters = new();
    public PhysicsMaterial2D DefaultPhyscsMaterial;
    public PhysicsMaterial2D OnFallenPhysicsMaterial;
    public PhysicsMaterial2D OnNotOnFloorPhysicsMaterial;

    const float COLLISION_HIT_DETECION_THICKNESS = 0.1f;
    const float COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET = 0.7f; //value between 0 and 1

    public event EventHandler<OnCollisionChangedEventArgs> OnCollisionChanged;
    public event EventHandler<AbstractCharacterComponent> OnHitOtherCharacters;

    private ZIndexLayer _currentZLayer;
    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;
    private Vector2 _positionPrevFrame;

    private GameObject _colliderFromFloor = null;
    private GameObject _colliderFromRoof = null;
    private GameObject _colliderFromLeftWall = null;
    private GameObject _colliderFromRightWall = null;


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
    public ZIndexLayer CurrentZLayer
    {
        get => _currentZLayer;
        private set => _currentZLayer = value;
    }
    public Vector3 PositionPrevFrame
    {
        get => _positionPrevFrame;
        private set => _positionPrevFrame = value;
    }
    
    public bool IsCollidingFloor()
    {
        return _colliderFromFloor != null;
    }
    public bool IsCollidingRoof()
    {
        return _colliderFromRoof != null;
    }
    public bool IsCollidingLeftWall()
    {
        return _colliderFromLeftWall != null;
    }
    public bool IsCollidingRightWall()
    {
        return _colliderFromRightWall != null;
    }

    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromFloor()
    {
        return _colliderFromFloor?.GetComponent<TileBehaviour>()?.BehaviourType;
    }
    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromRoof()
    {
        return _colliderFromRoof?.GetComponent<TileBehaviour>()?.BehaviourType;
    }
    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromLeftWall()
    {
        return _colliderFromLeftWall?.GetComponent<TileBehaviour>()?.BehaviourType;
    }
    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromRightWall()
    {
        return _colliderFromRightWall?.GetComponent<TileBehaviour>()?.BehaviourType;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        PositionPrevFrame = transform.position;
    }

    public bool GetIsStickingOnWall()
    {
        return
            CharComponents.CharacterInteractionWithTiles.IsAbleToStickOnWalls &&
            (
                GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY ||
                GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY
            );
    }

    private GameObject RaycastHitFromCollider(Vector2 from, Vector2 align)
    {
        float rayCastHitRange = 
            (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? 
                CharComponents.CharacterRigidBodyCapsuleCollider.size.x * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x : 
                CharComponents.CharacterRigidBodyCapsuleCollider.size.y * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y
            ) / 2 + COLLISION_HIT_DETECION_THICKNESS;

        //Debug.DrawLine(from, from + align * rayCastHitRange, Color.green);
        return Physics2D.Raycast(from, align, rayCastHitRange, 1 << _currentZLayer.EnviromentLayer).collider?.gameObject;
    }

    private GameObject RaycastHitFromCenter(Vector2 align)
    {
        Vector2 rayCastHitOrigin = VectorMath.Vec3ToVec2(transform.position) + CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale;
        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }

    private GameObject RaycastHitFromHead(Vector2 align)
    {
        float extraOffset = math.abs(
            CharComponents.CharacterRigidBodyCapsuleCollider.size.y * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y - 
            CharComponents.CharacterRigidBodyCapsuleCollider.size.x * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x
            ) / 2;
        Vector2 extraOffsetVec2 = CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? new Vector2(0f, extraOffset) : new Vector2(extraOffset, 0f);

        Vector2 rayCastHitOrigin =
            VectorMath.Vec3ToVec2(transform.position) +
            (CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale + extraOffsetVec2);

        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }

    private GameObject RaycastHitFromLegs(Vector2 align)
    {
        float extraOffset = math.abs(
            CharComponents.CharacterRigidBodyCapsuleCollider.size.y * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y - 
            CharComponents.CharacterRigidBodyCapsuleCollider.size.x * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x
            ) / 2;

        Vector2 extraOffsetVec2 = CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? new Vector2(0f, extraOffset) : new Vector2(extraOffset, 0f);

        Vector2 rayCastHitOrigin =
            VectorMath.Vec3ToVec2(transform.position) + 
            (CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale - extraOffsetVec2);

        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }


    private GameObject UpdateTileCollidingFromDirection(Vector2 direction)
    {
        return RaycastHitFromCenter(direction);
    }

    private GameObject UpdateTileCollidingFromFloor()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return RaycastHitFromLegs(Vector2.down);
        }
        else
        {
            return
                RaycastHitFromCenter(Vector2.down) ??
                RaycastHitFromHead(Vector2.down) ??
                RaycastHitFromLegs(Vector2.down);
        }
    }
    private GameObject UpdateTileCollidingFromRoof()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return RaycastHitFromHead(Vector2.up);
        }
        else
        {
            return
                RaycastHitFromCenter(Vector2.up) ??
                RaycastHitFromHead(Vector2.up) ??
                RaycastHitFromLegs(Vector2.up);
        }
    }
    private GameObject UpdateTileCollidingFromLeftWall()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return
                RaycastHitFromCenter(Vector2.left) ??
                RaycastHitFromHead(Vector2.left) ??
                RaycastHitFromLegs(Vector2.left);
        }
        else
        {
            return RaycastHitFromLegs(Vector2.left);
        }
    }
    private GameObject UpdateTileCollidingFromRightWall()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return
                RaycastHitFromCenter(Vector2.right) ??
                RaycastHitFromHead(Vector2.right) ??
                RaycastHitFromLegs(Vector2.right);
        }
        else
        {
            return RaycastHitFromHead(Vector2.right);
        }
    }

    private void FixedUpdate()
    {
        UpdateCurrentZLayer();
        UpdateTileCollidingInfo();
        UpdateTimeOnAirOrGround();
        UpdateHitVelocity();
        UpdatePhysicsMaterial();
    }

    private void UpdateCurrentZLayer()
    {
        _currentZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    private void UpdateTimeOnAirOrGround()
    {
        if (IsCollidingFloor())
        {
            _timeOnGround += Time.fixedDeltaTime;
            _timeInAir = 0f;
        }
        else
        {
            _timeInAir += Time.fixedDeltaTime;
            _timeOnGround = 0f;
        }
    }

    private void UpdateTileCollidingInfo()
    {
        GameObject prevColliderFromFloor = _colliderFromFloor;
        GameObject prevColliderFromRoof = _colliderFromRoof;
        GameObject prevColliderFromLeftWall = _colliderFromLeftWall;
        GameObject prevColliderFromRightWall = _colliderFromRightWall;

        _colliderFromFloor = UpdateTileCollidingFromFloor();
        _colliderFromRoof = UpdateTileCollidingFromRoof();
        _colliderFromLeftWall = UpdateTileCollidingFromLeftWall();
        _colliderFromRightWall = UpdateTileCollidingFromRightWall();

        if (prevColliderFromFloor != _colliderFromFloor) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromFloor != null, Vector2.down, _colliderFromFloor));
        if (prevColliderFromRoof != _colliderFromRoof) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromRoof != null, Vector2.down, _colliderFromRoof));
        if (prevColliderFromLeftWall != _colliderFromLeftWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromLeftWall != null, Vector2.down, _colliderFromLeftWall));
        if (prevColliderFromRightWall != _colliderFromRightWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromRightWall != null, Vector2.down, _colliderFromRightWall));
    }

    public void UpdateHitVelocity()
    {
        if (
            VectorMath.Vec2ToDistance(CharComponents.CharacterRigidBody.linearVelocity) >= SpeedToHitOtherCharacters && 
            (
                (CanHitWhileHardStnned && CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>()) ||
                (CanHitWhileRolling && CharComponents.CharacterRolling.IsRolling) ||
                CanHitWhileMoving
            )
            )
        {
            foreach (RaycastHit2D hit in Physics2D.LinecastAll(CharComponents.Center.transform.position, CharComponents.Center.PositionPreviousFrame, 1 << CurrentZLayer.CharactersLayer))
            {
                if (
                    hit.collider.TryGetComponent(out AbstractCharacterComponent otherCharComponent) &&
                    otherCharComponent.CharComponents.CharacterCollision != this &&
                    !CharComponents.CharacterEffectsReceiver.GetCharacterIsLastSender(otherCharComponent)
                    )
                {
                    //hit self
                    Vector2 affectingVelocity = CharComponents.CharacterRigidBody.linearVelocity / 2f;
                    CharComponents.CharacterRigidBody.linearVelocity -= affectingVelocity;
                    CharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffectsOnHitOtherCharacters, otherCharComponent);
                    //hit other character
                    otherCharComponent.CharComponents.CharacterRigidBody.linearVelocity += affectingVelocity;
                    otherCharComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnHitOtherCharacters, this);

                    OnHitOtherCharacters?.Invoke(this, otherCharComponent);
                }
            }
        }
    }

    public Vector2 GetColliderSize()
    {
        return CharComponents.CharacterRigidBodyCapsuleCollider.size;
    }

    private void UpdatePhysicsMaterial()
    {
        if (!IsCollidingFloor())
        {
            if (CharComponents.CharacterEffectsReceiver.GetHasEffect<AbstractStun>())
            {
                CharComponents.CharacterRigidBody.sharedMaterial = OnFallenPhysicsMaterial;
            }
            else
            {
                CharComponents.CharacterRigidBody.sharedMaterial = OnNotOnFloorPhysicsMaterial;
            }
        }
        else
        {
            CharComponents.CharacterRigidBody.sharedMaterial = DefaultPhyscsMaterial;
        }
    }


    private void LateUpdate()
    {
        _wasGroundedPrevFrame = IsCollidingFloor();
        PositionPrevFrame = transform.position;
    }
}
