using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class CharacterHoldingObjects : AbstractCharacterComponent
{
    const float DISARM_DROP_VELOCITY_MULTIPLIER = 0.1f;
    const float DISTANCE_TO_CAMERA_TO_DISABLE_HOLDABLE_WALL_COLLISION = 50f;
    const float PICKUP_SPEED_MULTIPLIER = 15f;

    public class OnThewEventArgs
    {
        public OnThewEventArgs(Holdable thrownObject, Vector2 direction)
        {
            ThrownObject = thrownObject;
            Direction = direction;
        }

        public Holdable ThrownObject;
        public Vector2 Direction;
    }

    [SerializeField] private bool _isAbleToGrabObjects = true;
    [SerializeField] private bool _isAbleToThrowObjects = true;
    [SerializeField] private bool _canDisarm = false;
    [SerializeField] private bool _throwObjectsOnStun = true;
    [SerializeField] private bool _throwObjectsOnDeath = true;
    [SerializeField] private Holdable _currentHoldObject = null;
    public float ThrowForce = 10f;
    public float MaxGrabRangeMultiplier = 1f;

    private Holdable _lastHoldObject = null;
    private float? _overrideHoldObjectDistance = null;

    public event EventHandler<OnThewEventArgs> OnThrewHoldable;
    public event EventHandler<Holdable> OnPickedUpHoldable;

    public Holdable CurrentHoldObject
    {
        get => _currentHoldObject;
        set
        {
            if (_currentHoldObject != null)
            {
                _lastHoldObject = _currentHoldObject;
            }
            _currentHoldObject = value;
        }
    }

    public Holdable LastHoldObject
    {
        get => _lastHoldObject;
        private set => _lastHoldObject = value;
    }

    /// <summary>
    /// Same as IsAbleToGrabObjects, but character will drop items if is sat to false
    /// </summary>
    public bool IsAbleToHoldObjects
    {
        get => _isAbleToGrabObjects;
        set
        {
            if (!value)
            {
                if (CharComponents.CharacterRigidBody.linearVelocity != Vector2.zero)
                {
                    ForceThrow(CharComponents.CharacterRigidBody.linearVelocity.normalized);
                }
                else
                {
                    ForceThrow(CharComponents.CharacterVisual.FlippedH ? Vector2.left : Vector2.right, 0.25f);
                }
            }
            _isAbleToGrabObjects = value;
        }
    }

    public bool ThrowObjectsOnStun
    {
        get => _throwObjectsOnStun;
        set
        {
            _throwObjectsOnStun = value;
            if (CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>())
            {
                if (value)
                {
                    ForceStunThrow();
                }
            }
        }
    }

    public bool ThrowObjectsOnDeath
    {
        get => _throwObjectsOnDeath;
        set
        {
            _throwObjectsOnDeath = value;
            if (CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>())
            {
                if (value)
                {
                    ForceStunThrow();
                }
                else
                {
                    CharComponents.CharacterAiming.AimWeaponDown = true;
                }
            }
        }
    }

    public bool IsAbleToGrabObjects
    {
        get => _isAbleToGrabObjects;
        set => _isAbleToGrabObjects = value;
    }

    public bool IsAbleToThrowObjects
    {
        get => _isAbleToThrowObjects;
        set => _isAbleToThrowObjects = value;
    }

    public bool CanDisarm
    {
        get => _canDisarm;
        set => _canDisarm = value;
    }

    public List<Holdable> GetAvaibleHoldables()
    {
        List<Holdable> result = new();
        foreach (Transform holdableTransform in CharComponents.CharacterCollision.CurrentZLayer.HoldablesContainer)
        {
            if (
                holdableTransform.TryGetComponent(out Holdable holdable) &&
                holdable.GetIsValidToInteract(CharComponents.gameObject) &&
                Vector2.Distance(CharComponents.Center.transform.position, holdableTransform.transform.position) <=
                    CharComponents.CharacterInteract.InteractRange * CharComponents.CharacterHolding.MaxGrabRangeMultiplier * holdable.SelectMaxRangeMultiplier
                )
            {
                result.Add(holdable);
            }
        }

        return result;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        if (CurrentHoldObject != null)
        {
            ForceGrab(CurrentHoldObject);
        }
        CharComponents.CharacterCollision.OnZIndexLayerChanged += CharacterCollision_OnZIndexLayerChanged;
    }

    private void CharacterCollision_OnZIndexLayerChanged(object sender, ZIndexLayer e)
    {
        if (_currentHoldObject != null)
        {
            LayerManager.Instance.ChangeZIndexForGameObject(LayerManager.Instance.GetZLayerOfGameObject(gameObject), _currentHoldObject.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (
            _currentHoldObject != null &&
            _currentHoldObject.HoldDistanceWhenIsHolded > 0.05f &&
            Vector2.Distance(Camera.main.transform.position, _currentHoldObject.transform.position) < DISTANCE_TO_CAMERA_TO_DISABLE_HOLDABLE_WALL_COLLISION
            )
        {
            Vector2 currentAim = CharComponents.CharacterAiming.GetCurrentAimNormalized();
            BoxCollider2D holdableCollider = _currentHoldObject.GetComponent<BoxCollider2D>();
            float holdableColliderLength = holdableCollider.size.x / 2 + holdableCollider.offset.x;

            //setting current holded object's location
            RaycastHit2D hit = Physics2D.Raycast(
                CharComponents.Center.transform.position,
                currentAim,
                _currentHoldObject.HoldDistanceWhenIsHolded + holdableColliderLength,
                1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EnviromentLayer
                );

            _overrideHoldObjectDistance = hit.collider != null ? hit.distance - holdableCollider.size.x / 2 : null;

        }
        else
        {
            _overrideHoldObjectDistance = null;
        }
    }

    private void Update()
    {
        if (_currentHoldObject == null) return;

        Vector2 currentAim = CharComponents.CharacterAiming.GetCurrentAimNormalized();
        Vector3 targetRotation = VectorMath.Vec2ToQuarterninon2D(currentAim).eulerAngles;

        if (CharComponents.CharacterAiming != null && CharComponents.CharacterAiming.IsAbleToAim)
        {
            //setting current holded object's rotation
            if (_currentHoldObject.RotatableWhenIsHolded)
            {

                Quaternion targetAngle = VectorMath.Vec2ToQuarterninon2D(currentAim);
                Vector3 targetEulerAngle = targetAngle.eulerAngles;
                targetAngle.eulerAngles = new Vector3(
                    targetEulerAngle.x,
                    math.lerp(
                        _currentHoldObject.transform.rotation.eulerAngles.y,
                        currentAim.x < 0f ? 180f : 0f,
                        CharComponents.CharacterAiming.AimSpeed * Time.deltaTime
                        ),
                    targetEulerAngle.z
                    );

                _currentHoldObject.transform.rotation = targetAngle;
            }
            else
            {
                _currentHoldObject.transform.localScale = new Vector3(
                    math.abs(_currentHoldObject.transform.localScale.x) * (CharComponents.CharacterVisual.FlippedH ? -1f : 1f),
                    _currentHoldObject.transform.localScale.y,
                    _currentHoldObject.transform.localScale.z
                    );
            }
        }

        Vector2 holdObjectPositionXY = Vector2.Lerp(
            _currentHoldObject.transform.position + (transform.position - CharComponents.CharacterCollision.PositionPrevFrame),
            VectorMath.Vec3ToVec2(CharComponents.Center.transform.position) + currentAim * _overrideHoldObjectDistance.GetValueOrDefault(_currentHoldObject.HoldDistanceWhenIsHolded),
            CharComponents.CharacterAiming.AimSpeed * Time.fixedDeltaTime
            );

        _currentHoldObject.transform.position = new Vector3(
            holdObjectPositionXY.x,
            holdObjectPositionXY.y,
            CharComponents.Center.transform.position.z
            );
    }

    public bool TryThrow(Vector2 align, float throwForceMultiplier = 1f)
    {
        if (IsAbleToThrowObjects)
        {
            return ForceThrow(align, throwForceMultiplier);
        }
        else
        {
            return false;
        }
    }

    public bool ForceStunThrow()
    {
        return ForceThrow(CharComponents.CharacterRigidBody.linearVelocity.normalized, 0.25f);
    }

    public bool TryDisarm(CharacterHoldingObjects giveDisarmedHoldableTo = null)
    {
        if (giveDisarmedHoldableTo != null && !giveDisarmedHoldableTo.CanDisarm)
        {
            return false;
        }
        else
        {
            return ForceDisarm();
        }
    }

    public bool ForceDisarm(CharacterHoldingObjects giveDisarmedHoldableTo = null)
    {
        if (ForceThrow(CharComponents.CharacterAiming.GetCurrentAimNormalized(), DISARM_DROP_VELOCITY_MULTIPLIER))
        {
            return giveDisarmedHoldableTo?.TryGrab(LastHoldObject) ?? true;
        }
        else
        {
            return false;
        }
    }

    public bool ForceThrow(Vector2 align, float throwForceMultiplier = 1f)
    {
        if (_currentHoldObject == null) return false;

        _currentHoldObject.Throw(align, throwForceMultiplier);

        OnThrewHoldable?.Invoke(this, new OnThewEventArgs(_currentHoldObject, align));

        return true;
    }

    public bool TryGrab(Holdable holdable, bool throwOldItem = false)
    {
        if (
            _isAbleToGrabObjects &&
            (throwOldItem || _currentHoldObject == null) &&
            Vector3.Distance(holdable.transform.position, transform.position) <= CharComponents.CharacterInteract.InteractRange * MaxGrabRangeMultiplier
            )
        {
            return ForceGrab(holdable);
        }
        else
        {
            return false;
        }
    }

    public bool ForceGrab(Holdable holdable)
    {
        holdable.Give(this);

        OnPickedUpHoldable?.Invoke(this, holdable);

        return true;
    }

    public void GiveNewHoldable(Holdable holdable)
    {
        if (holdable == null) return;

        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject);
        Holdable newHoldable = layer.TrySpawnObject(
            holdable.gameObject,
            CharComponents.Center.transform.position,
            null,
            null
            ).FirstOrDefault()?.GetComponent<Holdable>();

        if (newHoldable != null)
        {
            ForceGrab(newHoldable);
        }
    }

    private void OnDestroy()
    {
        CharComponents.CharacterCollision.OnZIndexLayerChanged -= CharacterCollision_OnZIndexLayerChanged;
    }
}
