using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterCollision : AbstractCharacterComponent
{
    const string ENVIROMENT_TAG_NAME = "Enviroment";
    const float COLLISION_DETECTION_PRECISSION = 0.05f;

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

    private ZIndexLayer _currentZLayer;
    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;
    private Vector2 _positionPrevFrame;
    private List<ContactPoint2D> _contacts = new();

    private GameObject _colliderFromFloor = null;
    private GameObject _colliderFromRoof = null;
    private GameObject _colliderFromLeftWall = null;
    private GameObject _colliderFromRightWall = null;

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

    public List<ContactPoint2D > Contacts
    {
        get => _contacts;
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

    private GameObject UpdateTileCollidingFromFloor()
    {
        foreach (ContactPoint2D contact in _contacts)
        {
            if (math.abs(contact.point.y - CharComponents.CharacterRigidBodyCapsuleCollider.bounds.min.y) < COLLISION_DETECTION_PRECISSION)
            {
                return contact.collider.gameObject;
            }
        }
        return null;
    }
    private GameObject UpdateTileCollidingFromRoof()
    {
        foreach (ContactPoint2D contact in _contacts)
        {
            if (math.abs(contact.point.y - CharComponents.CharacterRigidBodyCapsuleCollider.bounds.max.y) < COLLISION_DETECTION_PRECISSION)
            {
                return contact.collider.gameObject;
            }
        }
        return null;
    }
    private GameObject UpdateTileCollidingFromLeftWall()
    {
        foreach (ContactPoint2D contact in _contacts)
        {
            if (math.abs(contact.point.x - CharComponents.CharacterRigidBodyCapsuleCollider.bounds.min.x) < COLLISION_DETECTION_PRECISSION)
            {
                return contact.collider.gameObject;
            }
        }
        return null;
    }
    private GameObject UpdateTileCollidingFromRightWall()
    {
        foreach (ContactPoint2D contact in _contacts)
        {
            if (math.abs(contact.point.x - CharComponents.CharacterRigidBodyCapsuleCollider.bounds.max.x) < COLLISION_DETECTION_PRECISSION)
            {
                return contact.collider.gameObject;
            }
        }
        return null;
    }

    private void FixedUpdate()
    {
        UpdateCurrentZLayer();
        UpdateContacts();
        UpdateTileCollidingInfo();
        UpdateTimeOnAirOrGround();
        UpdateHitVelocity();
        UpdatePhysicsMaterial();
    }

    private void UpdateCurrentZLayer()
    {
        _currentZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    private void UpdateContacts()
    {
        CharComponents.CharacterRigidBodyCapsuleCollider.GetContacts(_contacts);
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
            GetHasEnoughVelocityToHit() && 
            (
                (CanHitWhileHardStnned && CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>()) ||
                (CanHitWhileRolling && CharComponents.CharacterRolling.IsRolling) ||
                CanHitWhileMoving
            )
            )
        {
            foreach (Transform character in _currentZLayer.CharactersContainer.transform)
            {
                if (
                    character.TryGetComponent(out AbstractCharacterComponent otherCharComponent) &&
                    Vector2.Distance(otherCharComponent.CharComponents.Center.transform.position, CharComponents.transform.position) < 
                        math.min(CharComponents.CharacterRigidBodyCapsuleCollider.size.x, CharComponents.CharacterRigidBodyCapsuleCollider.size.y) + COLLISION_DETECTION_PRECISSION &&
                    otherCharComponent.CharComponents.CharacterCollision != this &&
                    !otherCharComponent.CharComponents.CharacterCollision.GetHasEnoughVelocityToHit() &&
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
