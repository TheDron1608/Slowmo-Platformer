using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterHoldingObjects : AbstractCharacterComponent
{
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
    public float ThrowForce = 10f;
    public float MaxGrabRangeMultiplier = 1f;

    private Holdable _currentHoldObject = null;
    private Holdable _lastHoldObject = null;

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

    private void Update()
    {
        if (_currentHoldObject == null) return;
        if (CharComponents.CharacterAiming == null || !CharComponents.CharacterAiming.IsAbleToAim) return;

        float aimDelta = CharComponents.CharacterAiming.AimSpeed * Time.deltaTime;
        Vector2 currentAim = CharComponents.CharacterAiming.GetCurrentAimNormalized();
        Vector3 targetRotation = VectorMath.Vec2ToQuarterninon2D(currentAim).eulerAngles;

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
                    aimDelta
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
        
        //setting current holded object's location
        Vector2 holdObjectPositionXY = Vector2.Lerp(
            _currentHoldObject.transform.position + (transform.position - CharComponents.CharacterCollisionInfo.PositionPrevFrame),
            VectorMath.Vec3ToVec2(CharComponents.Center.transform.position) + currentAim * _currentHoldObject.HoldDistanceWhenIsHolded,
            aimDelta
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

    public bool ForceThrow(Vector2 align, float throwForceMultiplier = 1f)
    {
        if (_currentHoldObject == null) return false;

        _currentHoldObject.Throw(align, throwForceMultiplier);

        OnThrewHoldable?.Invoke(this, new OnThewEventArgs(_currentHoldObject, align));

        return true;
    }

    public bool TryGrab(Holdable holdable)
    {
        if (
            _isAbleToGrabObjects &&
            _currentHoldObject == null &&
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
}
