using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CharacterCollisionInfo : AbstractCharacterComponent
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

    const float COLLISION_HIT_DETECION_THICKNESS = 0.075f;
    const float COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET = 0.7f; //value between 0 and 1

    public event EventHandler<OnCollisionChangedEventArgs> OnCollisionChanged;
    public event EventHandler<OnTileBehavioutTypeCollisionChangedEventArgs> OnTileBehavioutTypeCollisionChanged;

    private ZIndexLayer _currentZLayer;
    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;

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
        float rayCastHitRange = (align.x != 0 ? CharComponents.CharacterRigidBodyCapsuleCollider.size.x : CharComponents.CharacterRigidBodyCapsuleCollider.size.y) / 2 + COLLISION_HIT_DETECION_THICKNESS;
        RaycastHit2D[] rayCastHits = Physics2D.RaycastAll(from, align, rayCastHitRange, 1 << _currentZLayer.EnviromentLayer);
        for (int i = 0; i < rayCastHits.Length; i++)
        {
            if (rayCastHits[i].collider.tag == ENVIROMENT_TAG_NAME) return rayCastHits[i];
        }
        return null;
    }

    private RaycastHit2D? RaycastHitFromCenter(Vector2 align)
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y) + CharComponents.CharacterRigidBodyCapsuleCollider.offset;
        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }

    private RaycastHit2D? RaycastHitFromHead(Vector2 align)
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y + (CharComponents.CharacterRigidBodyCapsuleCollider.size.y - CharComponents.CharacterRigidBodyCapsuleCollider.size.x * COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET) / 2) + CharComponents.CharacterRigidBodyCapsuleCollider.offset;
        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }

    private RaycastHit2D? RaycastHitFromLegs(Vector2 align)
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y - (CharComponents.CharacterRigidBodyCapsuleCollider.size.y - CharComponents.CharacterRigidBodyCapsuleCollider.size.x * COLLISION_HEAD_OR_LEGS_DECECTION_OFFSET) / 2) + CharComponents.CharacterRigidBodyCapsuleCollider.offset;
        return RaycastHitFromCollider(rayCastHitOrigin, align);
    }
    private bool UpdateIsCollidingFloor()
    {
        return RaycastHitFromCenter(Vector2.down) != null;
    }
    private bool UpdateIsCollidingRoof()
    {
        return RaycastHitFromCenter(Vector2.up) != null;
    }
    private bool UpdateIsCollidingLeftWall()
    {
        return (
            RaycastHitFromCenter(Vector2.left) != null ||
            RaycastHitFromHead(Vector2.left) != null ||
            RaycastHitFromLegs(Vector2.left) != null
            );
    }
    private bool UpdateIsCollidingRightWall()
    {
        return (
            RaycastHitFromCenter(Vector2.right) != null ||
            RaycastHitFromHead(Vector2.right) != null ||
            RaycastHitFromLegs(Vector2.right) != null
            );
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



    private void LateUpdate()
    {
        _wasGroundedPrevFrame = _isCollidingFloor;
    }
}
