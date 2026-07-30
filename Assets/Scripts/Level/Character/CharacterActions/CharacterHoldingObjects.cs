using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterHoldingObjects : AbstractCharacterComponent
{
    const float DISARM_DROP_VELOCITY_MULTIPLIER = 0.1f;
    const float DISTANCE_TO_CAMERA_TO_DISABLE_HOLDABLE_WALL_COLLISION = 50f;
    const float HOLSTERED_OBJECT_MOVE_SPEED_MULT = 50f;
    const float HOLSTERED_OBJECT_ROTATE_SPEED_MULT = 10f;
    const float TIME_SINCE_HOLSTERING_TO_INSTANT_MOVEMENT = 0.1f;

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
    [SerializeField] private Holdable _currentHolsteredHoldObject = null;
    public float ThrowForce = 10f;
    public float MaxGrabRangeMultiplier = 1f;
    public List<AbstractEffect> EffectsOnHoldedObject = new();
    public bool Telekinesis = false;
    public float TelekinesisDistance = 8f;
    public float TelekinesisForce = 5f;
    public float TelekinesisDurationSeconds = 1f;
    [SerializeField] private Transform _holsteredHoldObjectPosition;

    private Holdable _lastHoldObject = null;
    private Holdable _lastHolsteredHoldObject = null;
    private float? _overrideHoldObjectDistance = null;
    private Coroutine _telekinesisCoroutine = null;
    private float _timeSinceHoldingHoldable = 0f;
    private float _timeSinceHolsteringHoldable = 0f;

    public event EventHandler<OnThewEventArgs> OnThrewHoldable;
    public event EventHandler<Holdable> OnPickedUpHoldable;

    public Holdable CurrentHoldObject
    {
        get => _currentHoldObject;
        set
        {
            if (_currentHoldObject == value) return;

            if (_currentHoldObject != null)
            {
                _lastHoldObject = _currentHoldObject;
            }
            _timeSinceHoldingHoldable = 0f;
            _currentHoldObject = value;
        }
    }

    public Holdable CurrentHolsteredHoldObject
    {
        get => _currentHolsteredHoldObject;
        set
        {
            if (_currentHolsteredHoldObject == value) return;

            if (_currentHolsteredHoldObject != null)
            {
                _currentHolsteredHoldObject.IsHolstered = false;
                _lastHolsteredHoldObject = _currentHolsteredHoldObject;
                if (_currentHolsteredHoldObject.TryGetComponent(out SpriteRenderer sr))
                {
                    sr.flipX = false;
                    sr.flipY = false;
                }
            }
            if (value != null)
            {
                value.IsHolstered = true;
                if (value.TryGetComponent(out SpriteRenderer sr))
                {
                    if (value.FlipXOnHolstered == Holdable.HolsteredFlippingModes.FLIP_CONTANTLY)
                    {
                        sr.flipX = true;
                    }
                    if (value.FlipYOnHolstered == Holdable.HolsteredFlippingModes.FLIP_CONTANTLY)
                    {
                        sr.flipY = true;
                    }
                }
            }
            _timeSinceHolsteringHoldable = 0f;
            _currentHolsteredHoldObject = value;
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
        if (_currentHolsteredHoldObject != null)
        {
            LayerManager.Instance.ChangeZIndexForGameObject(LayerManager.Instance.GetZLayerOfGameObject(gameObject), _currentHolsteredHoldObject.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (
            _currentHoldObject != null &&
            _currentHoldObject.HoldDistanceWhenIsHolded + _currentHoldObject.ExtraHoldDistance > 0.05f &&
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
                _currentHoldObject.HoldDistanceWhenIsHolded + _currentHoldObject.ExtraHoldDistance + holdableColliderLength,
                1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject).EnviromentLayer
                );

            _overrideHoldObjectDistance = hit.collider != null ? hit.distance - holdableCollider.size.x / 2 - holdableCollider.offset.x : null;

        }
        else
        {
            _overrideHoldObjectDistance = null;
        }

        if (_currentHoldObject != null) _timeSinceHoldingHoldable += Time.deltaTime;
        if (_currentHolsteredHoldObject != null) _timeSinceHolsteringHoldable += Time.deltaTime;
    }

    private void Update()
    {
        if (_currentHoldObject != null)
        {
            Vector2 currentAim =
                _currentHoldObject.RotatableWhenIsHolded ?
                CharComponents.CharacterAiming.GetCurrentAimNormalized() :
                new Vector2(CharComponents.CharacterVisual.FlippedH ? -1f : 1f, 0f);

            Vector3 targetRotation = VectorMath.Vec2ToQuarterninon2D(currentAim).eulerAngles;

            if (CharComponents.CharacterAiming != null && CharComponents.CharacterAiming.IsAbleToAim)
            {
                //setting current holded object's rotation
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

            Vector2 holdObjectPositionXY = Vector2.Lerp(
                _currentHoldObject.transform.position + (transform.position - CharComponents.CharacterCollision.PositionPrevFrame),
                VectorMath.Vec3ToVec2(CharComponents.Center.transform.position) + currentAim * _overrideHoldObjectDistance.GetValueOrDefault(_currentHoldObject.HoldDistanceWhenIsHolded  + _currentHoldObject.ExtraHoldDistance),
                CharComponents.CharacterAiming.AimSpeed * Time.deltaTime
                );

            _currentHoldObject.transform.position = new Vector3(
                holdObjectPositionXY.x,
                holdObjectPositionXY.y,
                CharComponents.Center.transform.position.z
                );
        }

        if (_currentHolsteredHoldObject != null)
        {
            _currentHolsteredHoldObject.transform.position =
                _timeSinceHolsteringHoldable < TIME_SINCE_HOLSTERING_TO_INSTANT_MOVEMENT ?
                    math.lerp(
                        _currentHolsteredHoldObject.transform.position,
                        _holsteredHoldObjectPosition.position,
                        HOLSTERED_OBJECT_MOVE_SPEED_MULT * Time.deltaTime
                    ) :
                    _holsteredHoldObjectPosition.position;

            Quaternion targetAngle = _holsteredHoldObjectPosition.rotation;
            Vector3 targetEulerAngle = targetAngle.eulerAngles;
            targetEulerAngle.z += CharComponents.CharacterVisual.FlippedH ? 180f - _currentHolsteredHoldObject.AngleOnHolstered : _currentHolsteredHoldObject.AngleOnHolstered;
            targetEulerAngle.z = targetEulerAngle.z % 360f;

            if (_currentHolsteredHoldObject.TryGetComponent(out SpriteRenderer sr))
            {
                switch (_currentHolsteredHoldObject.FlipXOnHolstered)
                {
                    case Holdable.HolsteredFlippingModes.FLIP_ON_CHARACTER_FLIPPED:
                        sr.flipX = CharComponents.CharacterVisual.FlippedH;
                        break;
                    case Holdable.HolsteredFlippingModes.FLIP_ON_CHARACTER_FLIPPED_REVERSED:
                        sr.flipX = !CharComponents.CharacterVisual.FlippedH;
                        break;
                }

                switch (_currentHolsteredHoldObject.FlipYOnHolstered)
                {
                    case Holdable.HolsteredFlippingModes.FLIP_ON_CHARACTER_FLIPPED:
                        sr.flipY = CharComponents.CharacterVisual.FlippedH;
                        break;
                    case Holdable.HolsteredFlippingModes.FLIP_ON_CHARACTER_FLIPPED_REVERSED:
                        sr.flipY = !CharComponents.CharacterVisual.FlippedH;
                        break;
                }
            }

            targetAngle.eulerAngles = new Vector3(
                targetEulerAngle.x,
                targetEulerAngle.y,
                math.lerp(
                    _currentHolsteredHoldObject.transform.rotation.eulerAngles.z,
                    targetEulerAngle.z,
                    HOLSTERED_OBJECT_ROTATE_SPEED_MULT * Time.deltaTime
                    )
                );

            _currentHolsteredHoldObject.transform.rotation = targetAngle;
        }
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
            return giveDisarmedHoldableTo?.ForceGrab(LastHoldObject) ?? true;
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
            (throwOldItem || _currentHoldObject == null)
            )
        {
            if (holdable != null && Vector3.Distance(holdable.transform.position, transform.position) <= CharComponents.CharacterInteract.InteractRange * MaxGrabRangeMultiplier)
            {
                return ForceGrab(holdable);
            }
            else if (Telekinesis && _telekinesisCoroutine == null)
            {
                List<Holdable> affectedHoldables = new();
                foreach (Transform holdableTransform in LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesContainer)
                {
                    if (
                        holdableTransform.TryGetComponent(out Holdable avaibleHoldable) &&
                        avaibleHoldable.CurrentHolder == null &&
                        Vector2.Distance(CharComponents.Center.transform.position, holdableTransform.transform.position) < TelekinesisDistance
                        )
                    {
                        affectedHoldables.Add(avaibleHoldable);
                    }
                }
                if (affectedHoldables.Count > 0) _telekinesisCoroutine = StartCoroutine(TelekinesisHoldables(affectedHoldables));
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator TelekinesisHoldables(List<Holdable> holdables)
    {
        foreach (Holdable holdable in holdables)
        {
            holdable.TelekinesisAffector = this;
            holdable.StuckedToCollider = null;
            if (holdable.TryGetComponent(out Rigidbody2D holdableRB))
            {
                holdableRB.linearVelocity =
                    VectorMath.Vec3ToVec2(CharComponents.Center.transform.position - holdableRB.transform.position).normalized *
                    Vector2.Distance(CharComponents.Center.transform.position, holdableRB.transform.position) * TelekinesisForce;
            }
        }

        bool grabbed = false;
        for (float t = 0; t < TelekinesisDurationSeconds; t += Time.fixedDeltaTime)
        {
            if (!grabbed)
            {
                if (CurrentHoldObject != null) grabbed = true;

                foreach (Holdable holdable in holdables)
                {
                    if (
                        holdable != null && !holdable.IsDestroyed() &&
                        Vector3.Distance(holdable.transform.position, transform.position) <= CharComponents.CharacterInteract.InteractRange * MaxGrabRangeMultiplier
                        )
                    {
                        if (ForceGrab(holdable))
                        {
                            grabbed = true;
                        }
                    }
                }
            }

            yield return new WaitForFixedUpdate();
        }

        _telekinesisCoroutine = null;
    }

    public bool ForceGrab(Holdable holdable)
    {
        holdable.Give(this);

        OnPickedUpHoldable?.Invoke(this, holdable);

        return true;
    }

    public Holdable GiveNewHoldable(Holdable holdable)
    {
        if (holdable == null) return null;

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

        return newHoldable;
    }

    private void OnDestroy()
    {
        CharComponents.CharacterCollision.OnZIndexLayerChanged -= CharacterCollision_OnZIndexLayerChanged;
    }
}
