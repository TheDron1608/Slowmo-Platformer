using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    const float COLLISION_HIT_DETECION_THICKNESS = 0.05f;
    const float COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET = 0.7f; //value between 0 and 1

    public event EventHandler<OnCollisionChangedEventArgs> OnCollisionChanged;
    public event EventHandler<AbstractCharacterComponent> OnHitOtherCharacters;

    private ZIndexLayer _currentZLayer = null;
    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;
    private Vector2 _positionPrevFrame;
    private List<AbstractCharacterComponent> _currentCollidingCharacters = new();

    private GameObject _colliderFromFloor = null;
    private GameObject _colliderFromRoof = null;
    private GameObject _colliderFromLeftWall = null;
    private GameObject _colliderFromRightWall = null;

    private ForegroundRuleTile _collidedTileFromFloor = null;
    private ForegroundRuleTile _collidedTileFromRoof = null;
    private ForegroundRuleTile _collidedTileFromLeftWall = null;
    private ForegroundRuleTile _collidedTileFromRightWall = null;

    public event EventHandler<ZIndexLayer> OnZIndexLayerChanged;


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
        private set
        {
            if (_currentZLayer != value) OnZIndexLayerChanged?.Invoke(this, value);
            _currentZLayer = value;
        }
    }
    public Vector3 PositionPrevFrame
    {
        get => _positionPrevFrame;
        private set => _positionPrevFrame = value;
    }
    public List<AbstractCharacterComponent> CurrentCollidingCharacters
    {
        get => _currentCollidingCharacters;
        private set => _currentCollidingCharacters = value;
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

    public ForegroundRuleTile.ForegroundBehaviourType? GetTileBehaviourTypeFromFloor()
    {
        return _collidedTileFromFloor?.BehaviourType;
    }
    public ForegroundRuleTile.ForegroundBehaviourType? GetTileBehaviourTypeFromRoof()
    {
        return _collidedTileFromRoof?.BehaviourType;
    }
    public ForegroundRuleTile.ForegroundBehaviourType? GetTileBehaviourTypeFromLeftWall()
    {
        return _collidedTileFromLeftWall?.BehaviourType;
    }
    public ForegroundRuleTile.ForegroundBehaviourType? GetTileBehaviourTypeFromRightWall()
    {
        return _collidedTileFromRightWall?.BehaviourType;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        PositionPrevFrame = transform.position;
    }

    private void OnEnable()
    {
        _currentCollidingCharacters = new();
        _wasGroundedPrevFrame = true;
        UpdateCurrentZLayer();
    }

    public bool GetIsStickingOnWall()
    {
        return
            CharComponents.CharacterInteractionWithTiles.IsAbleToStickOnWalls &&
            (
                GetTileBehaviourTypeFromLeftWall() == ForegroundRuleTile.ForegroundBehaviourType.STICKY ||
                GetTileBehaviourTypeFromRightWall() == ForegroundRuleTile.ForegroundBehaviourType.STICKY
            );
    }

    private GameObject RaycastHitFromCollider(Vector2 from, Vector2 align, out ForegroundRuleTile collidedTile)
    {
        float rayCastHitRange = 
            (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? 
                CharComponents.CharacterRigidBodyCapsuleCollider.size.x * math.abs(CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x) : 
                CharComponents.CharacterRigidBodyCapsuleCollider.size.y * math.abs(CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y)
            ) / 2 + COLLISION_HIT_DETECION_THICKNESS;

        //Debug.DrawLine(from, from + align * rayCastHitRange, Color.green);
        Vector2 checkPosition = from + align * rayCastHitRange;
        GameObject result = Physics2D.OverlapPoint(checkPosition, 1 << _currentZLayer.EnviromentLayer)?.gameObject;
        if (result != null)
        {
            collidedTile = _currentZLayer.MultiTileMapsContainer.GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).GetTile<ForegroundRuleTile>(new Vector3Int((int)math.floor(checkPosition.x), (int)math.floor(checkPosition.y), 0));
            return result;
        }
        else
        {
            collidedTile = null;
            return null;
        }
    }

    private GameObject RaycastHitFromCenter(Vector2 align, out ForegroundRuleTile collidedTile)
    {
        Vector2 rayCastHitOrigin = VectorMath.Vec3ToVec2(transform.position) + CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale;
        return RaycastHitFromCollider(rayCastHitOrigin, align, out collidedTile);
    }

    private GameObject RaycastHitFromHead(Vector2 align, out ForegroundRuleTile collidedTile)
    {
        float extraOffset = math.abs(
            math.abs(CharComponents.CharacterRigidBodyCapsuleCollider.size.y * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y) -
            math.abs(CharComponents.CharacterRigidBodyCapsuleCollider.size.x * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x)
            ) / 2;
        Vector2 extraOffsetVec2 = CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? 
            new Vector2(0f, extraOffset) : 
            new Vector2(extraOffset, 0f);

        Vector2 rayCastHitOrigin =
            VectorMath.Vec3ToVec2(transform.position) +
            (CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale + extraOffsetVec2);

        return RaycastHitFromCollider(rayCastHitOrigin, align, out collidedTile);
    }

    private GameObject RaycastHitFromLegs(Vector2 align, out ForegroundRuleTile collidedTile)
    {
        float extraOffset = math.abs(
            math.abs(CharComponents.CharacterRigidBodyCapsuleCollider.size.y * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y) -
            math.abs(CharComponents.CharacterRigidBodyCapsuleCollider.size.x * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x)
            ) / 2;

        Vector2 extraOffsetVec2 = CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? 
            new Vector2(0f, extraOffset) :
            new Vector2(extraOffset, 0f);

        Vector2 rayCastHitOrigin =
            VectorMath.Vec3ToVec2(transform.position) + 
            (CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale - extraOffsetVec2);

        return RaycastHitFromCollider(rayCastHitOrigin, align, out collidedTile);
    }

    private GameObject UpdateTileCollidingFromFloor(out ForegroundRuleTile collidedTile)
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return RaycastHitFromLegs(Vector2.down, out collidedTile);
        }
        else
        {
            return
                RaycastHitFromCenter(Vector2.down, out collidedTile) ??
                RaycastHitFromHead(Vector2.down, out collidedTile) ??
                RaycastHitFromLegs(Vector2.down, out collidedTile);
        }
    }
    private GameObject UpdateTileCollidingFromRoof(out ForegroundRuleTile collidedTile)
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return RaycastHitFromHead(Vector2.up, out collidedTile);
        }
        else
        {
            return
                RaycastHitFromCenter(Vector2.up, out collidedTile) ??
                RaycastHitFromHead(Vector2.up, out collidedTile) ??
                RaycastHitFromLegs(Vector2.up, out collidedTile);
        }
    }
    private GameObject UpdateTileCollidingFromLeftWall(out ForegroundRuleTile collidedTile)
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return
                RaycastHitFromCenter(Vector2.left, out collidedTile) ??
                RaycastHitFromHead(Vector2.left, out collidedTile) ??
                RaycastHitFromLegs(Vector2.left, out collidedTile);
        }
        else
        {
            return RaycastHitFromLegs(Vector2.left, out collidedTile);
        }
    }
    private GameObject UpdateTileCollidingFromRightWall(out ForegroundRuleTile collidedTile)
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return
                RaycastHitFromCenter(Vector2.right, out collidedTile) ??
                RaycastHitFromHead(Vector2.right, out collidedTile) ??
                RaycastHitFromLegs(Vector2.right, out collidedTile);
        }
        else
        {
            return RaycastHitFromHead(Vector2.right, out collidedTile);
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
        CurrentZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
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

        _colliderFromFloor = UpdateTileCollidingFromFloor(out _collidedTileFromFloor);
        _colliderFromRoof = UpdateTileCollidingFromRoof(out _collidedTileFromRoof);
        _colliderFromLeftWall = UpdateTileCollidingFromLeftWall(out _collidedTileFromLeftWall);
        _colliderFromRightWall = UpdateTileCollidingFromRightWall(out _collidedTileFromRightWall);

        if (prevColliderFromFloor != _colliderFromFloor) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromFloor != null, Vector2.down, _colliderFromFloor));
        if (prevColliderFromRoof != _colliderFromRoof) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromRoof != null, Vector2.down, _colliderFromRoof));
        if (prevColliderFromLeftWall != _colliderFromLeftWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromLeftWall != null, Vector2.down, _colliderFromLeftWall));
        if (prevColliderFromRightWall != _colliderFromRightWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_colliderFromRightWall != null, Vector2.down, _colliderFromRightWall));
    }

    public void UpdateHitVelocity()
    {
        CurrentCollidingCharacters = new();
        float hitRadius = (CharComponents.CharacterRigidBodyCapsuleCollider.size.x + CharComponents.CharacterRigidBodyCapsuleCollider.size.y) / 2;

        foreach (Transform otherCharacterTransform in _currentZLayer.CharactersContainer)
        {
            if (
                otherCharacterTransform.gameObject.activeSelf &&
                otherCharacterTransform.TryGetComponent(out AbstractCharacterComponent otherCharComponent) &&
                Vector2.Distance(otherCharComponent.CharComponents.Center.transform.position, CharComponents.Center.transform.position) < hitRadius &&
                otherCharComponent.CharComponents.CharacterCollision != this &&
                !otherCharComponent.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
                )
            {
                CurrentCollidingCharacters.Add(otherCharComponent);

                if (
                    (
                        CanHitWhileMoving &&
                        CharComponents.CharacterMoving.GetCurrentMoveDirection() != 0f
                    ) ||
                    (
                        CanHitWhileHardStnned &&
                        CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>() &&
                        !CharComponents.CharacterEffectsReceiver.GetCharacterIsLastSender(otherCharComponent) &&
                        GetHasEnoughVelocityToHit()
                    ) ||
                    (
                        CanHitWhileRolling &&
                        CharComponents.CharacterRolling.IsRolling &&
                        !CharComponents.CharacterRolling.CurrentRollHitCharacters.Contains(otherCharComponent)
                    )
                    ) 
                {
                    //hit self
                    Vector2 affectingVelocity = CharComponents.CharacterRigidBody.linearVelocity / 2f;
                    CharComponents.CharacterRigidBody.linearVelocity -= affectingVelocity;
                    CharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffectsOnHitOtherCharacters, otherCharComponent);
                    //hit other character
                    otherCharComponent.CharComponents.CharacterRigidBody.linearVelocity += affectingVelocity;
                    otherCharComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnHitOtherCharacters, this);

                    if (CharComponents.CharacterRolling.IsRolling)
                    {
                        CharComponents.CharacterRolling.CurrentRollHitCharacters.Add(otherCharComponent);
                    }
                    OnHitOtherCharacters?.Invoke(this, otherCharComponent);
                }
            }
        }
    }

    public bool GetHasEnoughVelocityToHit()
    {
        return VectorMath.Vec2ToDistance(CharComponents.CharacterRigidBody.linearVelocity) >= SpeedToHitOtherCharacters;
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
