using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterCollision : AbstractCharacterComponent
{
    const string ENVIROMENT_TAG_NAME = "Enviroment";
    const float COLLISION_DETECTION_THICKNESS = 0.05f;
    const float CHECK_COLLIDING_INTERACTABLE_FURNITURE_DISTANCE = 3f;

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
    private Vector2 _positionPrevFrame;
    private List<AbstractCharacterComponent> _currentCollidingCharacters = new();
    private List<Collider2D> _currentNearbyCollidableFurniture = new();
    private AbstractCharacterComponent _encountKillOnOutOfMapCharacter = null;

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
        CharComponents.CharacterEffectsReceiver.OnEffectAdded += CharacterEffectsReceiver_OnEffectAdded;
    }

    private void CharacterEffectsReceiver_OnEffectAdded(object sender, ObjectEffectsReceiver.EffectAddedEventArgs e)
    {
        if (e.Effect is Knockback || e.Effect is AbstractStun)
        {
            AbstractCharacterComponent senderCharacter = ObjectEffectsReceiver.TryGetCharacterFromSender(e.Sender);
            if (
                senderCharacter != null && 
                senderCharacter.CharComponents != CharComponents && 
                !senderCharacter.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(CharComponents.CharacterTeam)
                )
            {
                _encountKillOnOutOfMapCharacter = senderCharacter;
            }
        }
    }

    private void OnEnable()
    {
        _currentCollidingCharacters = new();
        UpdateCurrentZLayer();
    }

    public bool GetIsStickingOnWall()
    {
        return
            CharComponents.CharacterInteractionWithTiles.IsCurrentAbleToStickOnWalls &&
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
            collidedTile = _currentZLayer.MultiTileMapsContainer.GetForeground().GetTile<ForegroundRuleTile>(new Vector3Int((int)math.floor(checkPosition.x), (int)math.floor(checkPosition.y), 0));
            return result;
        }
        else
        {
            collidedTile = null;
            return null;
        }
    }

    private GameObject UpdateTileCollidingAtPoint(Vector2 pointCenter, out ForegroundRuleTile collidedTile)
    {
        Tilemap foregroundTilemap = _currentZLayer.MultiTileMapsContainer.GetForeground();
        collidedTile = foregroundTilemap.GetTile<ForegroundRuleTile>(
            new Vector3Int((int)math.floor(pointCenter.x), (int)math.floor(pointCenter.y))
            );
        foreach (Collider2D collider in _currentNearbyCollidableFurniture)
        {
            if (collider.OverlapPoint(pointCenter))
            {
                return collider.gameObject;
            }
        }

        return collidedTile != null ? foregroundTilemap.gameObject : null;
    }
    private GameObject UpdateTileCollidingAtMultiPoint(Vector2 pointCenter, Vector2 pointLeft, Vector2 pointRight, out ForegroundRuleTile collidedTile)
    {
        Tilemap foregroundTilemap = _currentZLayer.MultiTileMapsContainer.GetForeground();
        collidedTile =
            foregroundTilemap.GetTile<ForegroundRuleTile>(new Vector3Int((int)math.floor(pointCenter.x), (int)math.floor(pointCenter.y), 0)) ??
            foregroundTilemap.GetTile<ForegroundRuleTile>(new Vector3Int((int)math.floor(pointLeft.x), (int)math.floor(pointLeft.y), 0)) ??
            foregroundTilemap.GetTile<ForegroundRuleTile>(new Vector3Int((int)math.floor(pointRight.x), (int)math.floor(pointRight.y), 0));

        foreach (Collider2D collider in _currentNearbyCollidableFurniture)
        {
            if (collider.OverlapPoint(pointCenter))
            {
                return collider.gameObject;
            }
        }

        return collidedTile != null ? foregroundTilemap.gameObject : null;
    }

    private GameObject UpdateTileCollidingFromFloor(out ForegroundRuleTile collidedTile)
    {
        Bounds bounds = CharComponents.CharacterRigidBodyCapsuleCollider.bounds;
        return UpdateTileCollidingAtMultiPoint(
            new Vector2(
                bounds.center.x,
                bounds.min.y - COLLISION_DETECTION_THICKNESS
                ),
            new Vector2(
                bounds.min.x,
                bounds.min.y - COLLISION_DETECTION_THICKNESS
                ),
            new Vector2(
                bounds.max.x,
                bounds.min.y - COLLISION_DETECTION_THICKNESS
                ),
            out collidedTile
            );
    }
    private GameObject UpdateTileCollidingFromRoof(out ForegroundRuleTile collidedTile)
    {
        Bounds bounds = CharComponents.CharacterRigidBodyCapsuleCollider.bounds;
        return UpdateTileCollidingAtPoint(
            new Vector2(
                bounds.center.x,
                bounds.max.y + COLLISION_DETECTION_THICKNESS
                ),
            out collidedTile
            );
    }
    private GameObject UpdateTileCollidingFromLeftWall(out ForegroundRuleTile collidedTile)
    {
        Bounds bounds = CharComponents.CharacterRigidBodyCapsuleCollider.bounds;
        return UpdateTileCollidingAtPoint(
            new Vector2(
                bounds.min.x - COLLISION_DETECTION_THICKNESS,
                bounds.center.y
                ),
            out collidedTile
            );
    }
    private GameObject UpdateTileCollidingFromRightWall(out ForegroundRuleTile collidedTile)
    {
        Bounds bounds = CharComponents.CharacterRigidBodyCapsuleCollider.bounds;
        return UpdateTileCollidingAtPoint(
            new Vector2(
                bounds.max.x + COLLISION_DETECTION_THICKNESS,
                bounds.center.y
                ),
            out collidedTile
            );
    }

    private void FixedUpdate()
    {
        UpdateCurrentZLayer();
        UpdateNearbyCollidableFuniture();
        UpdateTileCollidingInfo();
        UpdateEncountKillOnOutOfMapCharacter();
        UpdateIsOutFromMapBottom();
        UpdateTimeOnAirOrGround();
        UpdateHitVelocity();
        UpdatePhysicsMaterial();
    }

    private void UpdateCurrentZLayer()
    {
        CurrentZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    private void UpdateNearbyCollidableFuniture()
    {
        _currentNearbyCollidableFurniture = new();
        foreach (Transform furniture in _currentZLayer.InteractableEnviromentContainer)
        {
            if (
                Vector2.Distance(CharComponents.Center.transform.position, furniture.transform.position) < CHECK_COLLIDING_INTERACTABLE_FURNITURE_DISTANCE &&
                furniture.gameObject.layer == _currentZLayer.EnviromentLayer &&   
                furniture.TryGetComponent(out Collider2D collider)
                )
            {
                _currentNearbyCollidableFurniture.Add(collider);
            }
        }
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

    private void UpdateEncountKillOnOutOfMapCharacter()
    {
        if (
            _encountKillOnOutOfMapCharacter != null && 
            IsCollidingFloor() && 
            !CharComponents.CharacterEffectsReceiver.GetHasEffect<AbstractStun>()
            )
        {
            _encountKillOnOutOfMapCharacter = null;
        }
    }

    private void UpdateIsOutFromMapBottom()
    {
        if (CharComponents.Center.transform.position.y < LayerManager.Instance.GetLevelBottom() && !CharComponents.CharacterVisual.GetIsVisible())
        {
            CharComponents.CharacterHealth.Die(_encountKillOnOutOfMapCharacter, null);
            Destroy(CharComponents.gameObject);
        }
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
                    CharComponents.CharacterEffectsReceiver.ApplyEffect(SelfEffectsOnHitOtherCharacters, otherCharComponent, 1f, true);
                    //hit other character
                    otherCharComponent.CharComponents.CharacterRigidBody.linearVelocity += affectingVelocity;
                    otherCharComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnHitOtherCharacters, this);

                    if (CharComponents.CharacterRolling.IsRolling)
                    {
                        CharComponents.CharacterRolling.CurrentRollHitCharacters.Add(otherCharComponent);
                    }

                    CharComponents.CharacterRolling.SoundOnRollHit.PlaySound();
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
        PositionPrevFrame = transform.position;
    }

    private void OnDestroy()
    {
        if (CharComponents.CharacterEffectsReceiver != null)
        {
            CharComponents.CharacterEffectsReceiver.OnEffectAdded -= CharacterEffectsReceiver_OnEffectAdded;
        }
    }
}
