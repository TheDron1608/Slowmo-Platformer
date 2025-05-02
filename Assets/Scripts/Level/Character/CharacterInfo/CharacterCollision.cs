using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterCollision : AbstractCharacterComponent
{
    const string ENVIROMENT_TAG_NAME = "Enviroment";

    public class OnCollisionChangedEventArgs
    {
        public OnCollisionChangedEventArgs(bool enterOrReleasedCollision, Vector2 collisionAlign)
        {
            EnterOrReleasedCollision = enterOrReleasedCollision;
            CollisionAlign = collisionAlign;
        }

        /// <summary>
        /// If true enteres collision, else releases collision
        /// </summary>
        public bool EnterOrReleasedCollision;
        public Vector2 CollisionAlign;
    }
    public class OnTileBehavioutTypeCollisionChangedEventArgs
    {
        public OnTileBehavioutTypeCollisionChangedEventArgs(TileBehaviour.TileBehaviourType? behaviourType, Vector2 collisionAlign)
        {
            BehaviourType = behaviourType;
            CollisionAlign = collisionAlign;
        }

        public TileBehaviour.TileBehaviourType? BehaviourType;
        public Vector2 CollisionAlign;
    }

    public float SpeedToHitOtherCharacters = 12.5f;
    public bool CanHitWhileHardStnned = true;
    public bool CanHitWhileMoving = false;
    public bool CanHitWhileRolling = false;
    public List<AbstractCharacterEffect> EffectsOnHitOtherCharacters = new();
    public List<AbstractCharacterEffect> SelfEffectsOnHitOtherCharacters = new();
    public PhysicsMaterial2D DefaultPhyscsMaterial;
    public PhysicsMaterial2D OnFallenPhysicsMaterial;

    const float COLLISION_HIT_DETECION_THICKNESS = 0.1f;
    const float COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET = 0.7f; //value between 0 and 1

    public event EventHandler<OnCollisionChangedEventArgs> OnCollisionChanged;
    public event EventHandler<OnTileBehavioutTypeCollisionChangedEventArgs> OnTileBehavioutTypeCollisionChanged;
    public event EventHandler<AbstractCharacterComponent> OnHitOtherCharacters;

    private ZIndexLayer _currentZLayer;
    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;
    private Vector2 _positionPrevFrame;

    private bool _isCollidingFloor = false;
    private bool _isCollidingRoof = false;
    private bool _isCollidingLeftWall = false;
    private bool _isCollidingRightWall = false;

    private TileBehaviour.TileBehaviourType? _behaviourTypeFromFloor = null;
    private TileBehaviour.TileBehaviourType? _behaviourTypeFromRoof = null;
    private TileBehaviour.TileBehaviourType? _behaviourTypeFromLeftWall = null;
    private TileBehaviour.TileBehaviourType? _behaviourTypeFromRightWall = null;


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
        return _isCollidingFloor;
    }
    public bool IsCollidingRoof()
    {
        return _isCollidingRoof;
    }
    public bool IsCollidingLeftWall()
    {
        return _isCollidingLeftWall;
    }
    public bool IsCollidingRightWall()
    {
        return _isCollidingRightWall;
    }

    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromFloor()
    {
        return _behaviourTypeFromFloor;
    }
    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromRoof()
    {
        return _behaviourTypeFromRoof;
    }
    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromLeftWall()
    {
        return _behaviourTypeFromLeftWall;
    }
    public TileBehaviour.TileBehaviourType? GetTileBehaviourTypeFromRightWall()
    {
        return _behaviourTypeFromRightWall;
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

    private RaycastHit2D? RaycastHitFromCollider(Vector2 from, Vector2 align)
    {
        float rayCastHitRange = 
            (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical ? 
                CharComponents.CharacterRigidBodyCapsuleCollider.size.x * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.x : 
                CharComponents.CharacterRigidBodyCapsuleCollider.size.y * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale.y
            ) / 2 + COLLISION_HIT_DETECION_THICKNESS;
        RaycastHit2D[] rayCastHits = Physics2D.RaycastAll(from, align, rayCastHitRange, 1 << _currentZLayer.EnviromentLayer);
        //Debug.DrawLine(from, from + align * rayCastHitRange);
        for (int i = 0; i < rayCastHits.Length; i++)
        {
            if (rayCastHits[i].collider.tag == ENVIROMENT_TAG_NAME) return rayCastHits[i];
        }
        return null;
    }

    private RaycastHit2D? RaycastHitFromCenter(Vector2 align)
    {
        Vector2 rayCastHitOrigin = VectorMath.Vec3ToVec2(transform.position) + CharComponents.CharacterRigidBodyCapsuleCollider.offset * CharComponents.CharacterRigidBodyCapsuleCollider.transform.localScale;
        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }

    private RaycastHit2D? RaycastHitFromHead(Vector2 align)
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

    private RaycastHit2D? RaycastHitFromLegs(Vector2 align)
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

    private bool UpdateIsCollidingFloor()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return RaycastHitFromLegs(Vector2.down) != null;
        }
        else
        {
            return
                RaycastHitFromCenter(Vector2.down) != null ||
                RaycastHitFromHead(Vector2.down) != null ||
                RaycastHitFromLegs(Vector2.down) != null;
        }
    }
    private bool UpdateIsCollidingRoof()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return RaycastHitFromHead(Vector2.up) != null;
        }
        else
        {
            return
                RaycastHitFromCenter(Vector2.up) != null ||
                RaycastHitFromHead(Vector2.up) != null ||
                RaycastHitFromLegs(Vector2.up) != null;
        }
    }
    private bool UpdateIsCollidingLeftWall()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return
                RaycastHitFromCenter(Vector2.left) != null ||
                RaycastHitFromHead(Vector2.left) != null ||
                RaycastHitFromLegs(Vector2.left) != null;
        }
        else
        {
            return RaycastHitFromLegs(Vector2.left) != null;
        }
    }
    private bool UpdateIsCollidingRightWall()
    {
        if (CharComponents.CharacterRigidBodyCapsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            return
                RaycastHitFromCenter(Vector2.right) != null ||
                RaycastHitFromHead(Vector2.right) != null ||
                RaycastHitFromLegs(Vector2.right) != null;
        }
        else
        {
            return RaycastHitFromHead(Vector2.right) != null;
        }
    }


    private TileBehaviour.TileBehaviourType? UpdateTileCollidingFromDirection(Vector2 direction)
    {
        RaycastHit2D? hitGameObject = RaycastHitFromCenter(direction);

        if (hitGameObject == null || !hitGameObject.Value.collider.gameObject.TryGetComponent<TileBehaviour>(out TileBehaviour hitGameObjectTileBehaviour))
        {
            return null;
        }
        else
        {
            return hitGameObjectTileBehaviour.BehaviourType;
        }
    }

    private TileBehaviour.TileBehaviourType? UpdateTileCollidingFromFloor()
    {
        return UpdateTileCollidingFromDirection(Vector2.down);
    }
    private TileBehaviour.TileBehaviourType? UpdateTileCollidingFromRoof()
    {
        return UpdateTileCollidingFromDirection(Vector2.up);
    }
    private TileBehaviour.TileBehaviourType? UpdateTileCollidingFromLeftWall()
    {
        return UpdateTileCollidingFromDirection(Vector2.left);
    }
    private TileBehaviour.TileBehaviourType? UpdateTileCollidingFromRightWall()
    {
        return UpdateTileCollidingFromDirection(Vector2.right);
    }

    private void FixedUpdate()
    {
        UpdateCurrentZLayer();
        UpdateCollidingInfo();
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

    private void UpdateCollidingInfo()
    {
        bool wasCollidingFloor = _isCollidingFloor;
        bool wasCollidingRoof = _isCollidingRoof;
        bool wasCollidingLeftWall = _isCollidingLeftWall;
        bool wasCollidingRightWall = _isCollidingRightWall;

        _isCollidingFloor = UpdateIsCollidingFloor();
        _isCollidingRoof = UpdateIsCollidingRoof();
        _isCollidingLeftWall = UpdateIsCollidingLeftWall();
        _isCollidingRightWall = UpdateIsCollidingRightWall();

        if (wasCollidingFloor != _isCollidingFloor) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingFloor, Vector2.down));
        if (wasCollidingRoof != _isCollidingRoof) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingRoof, Vector2.up));
        if (wasCollidingLeftWall != _isCollidingLeftWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingLeftWall, Vector2.left));
        if (wasCollidingRightWall != _isCollidingRightWall) OnCollisionChanged?.Invoke(this, new OnCollisionChangedEventArgs(_isCollidingRightWall, Vector2.right));
    }

    private void UpdateTileCollidingInfo()
    {
        TileBehaviour.TileBehaviourType? lastTypeFromFloor = _behaviourTypeFromFloor;
        TileBehaviour.TileBehaviourType? lastTypeFromRoof = _behaviourTypeFromRoof;
        TileBehaviour.TileBehaviourType? lastTypeFromLeftWall = _behaviourTypeFromLeftWall;
        TileBehaviour.TileBehaviourType? lastTypeFromRightWall = _behaviourTypeFromRightWall;

        _behaviourTypeFromFloor = UpdateTileCollidingFromFloor();
        _behaviourTypeFromRoof = UpdateTileCollidingFromRoof();
        _behaviourTypeFromLeftWall = UpdateTileCollidingFromLeftWall();
        _behaviourTypeFromRightWall = UpdateTileCollidingFromRightWall();

        if (lastTypeFromFloor != _behaviourTypeFromFloor) OnTileBehavioutTypeCollisionChanged?.Invoke(this, new OnTileBehavioutTypeCollisionChangedEventArgs(_behaviourTypeFromFloor, Vector2.down));
        if (lastTypeFromRoof != _behaviourTypeFromRoof) OnTileBehavioutTypeCollisionChanged?.Invoke(this, new OnTileBehavioutTypeCollisionChangedEventArgs(_behaviourTypeFromRoof, Vector2.up));
        if (lastTypeFromLeftWall != _behaviourTypeFromLeftWall) OnTileBehavioutTypeCollisionChanged?.Invoke(this, new OnTileBehavioutTypeCollisionChangedEventArgs(_behaviourTypeFromLeftWall, Vector2.left));
        if (lastTypeFromRightWall != _behaviourTypeFromRightWall) OnTileBehavioutTypeCollisionChanged?.Invoke(this, new OnTileBehavioutTypeCollisionChangedEventArgs(_behaviourTypeFromRightWall, Vector2.right));
    }

    public void UpdateHitVelocity()
    {
        if (
            VectorMath.Vec2ToDistance(CharComponents.CharacterRigidBody.linearVelocity) >= SpeedToHitOtherCharacters && 
            (
                (CanHitWhileHardStnned && CharComponents.CharacterEffects.GetHasEffect<HardStun>()) ||
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
                    !CharComponents.CharacterEffects.GetLastOneSecondHittersContainsCharacter(otherCharComponent) &&
                    !otherCharComponent.CharComponents.CharacterEffects.GetLastOneSecondHittersContainsCharacter(this)
                    )
                {
                    //hit self
                    Vector2 affectingVelocity = CharComponents.CharacterRigidBody.linearVelocity / 2f;
                    CharComponents.CharacterRigidBody.linearVelocity -= affectingVelocity;
                    CharComponents.CharacterEffects.ApplyEffect(SelfEffectsOnHitOtherCharacters, otherCharComponent, null);
                    //hit other character
                    otherCharComponent.CharComponents.CharacterRigidBody.linearVelocity += affectingVelocity;
                    otherCharComponent.CharComponents.CharacterEffects.ApplyEffect(EffectsOnHitOtherCharacters, this, null);

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
        if (!IsCollidingFloor() && CharComponents.CharacterEffects.GetHasEffect<AbstractStun>())
        {
            CharComponents.CharacterRigidBody.sharedMaterial = OnFallenPhysicsMaterial;
        }
        else
        {
            CharComponents.CharacterRigidBody.sharedMaterial = DefaultPhyscsMaterial;
        }
    }


    private void LateUpdate()
    {
        _wasGroundedPrevFrame = _isCollidingFloor;
        PositionPrevFrame = transform.position;
    }
}
