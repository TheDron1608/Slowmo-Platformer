using System;
using Unity.Mathematics;
using UnityEngine;

public class CharacterHoldingObjects : MonoBehaviour
{
    public float ThrowForce = 10f;
    public float MaxGrabRangeMultiplier = 1f;

    private Holdable _currentHoldObject = null;
    private Holdable _lastHoldObject = null;
    private bool _isAbleToGrabObjects;

    private Rigidbody2D _rigidBodyComponent;
    private CharacterVisual _characterVisualComponent;
    private CharacterActions _characterActionsComponent;
    private CharacterChildNodes _characterChildNodes;

    public event EventHandler<Holdable> OnHoldableChanged;

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _characterVisualComponent)) throw new UnityException("CharacterVisual component not found");
        if (!TryGetComponent(out _characterActionsComponent)) throw new UnityException("CharacterActions component not found");
        if (!TryGetComponent(out _characterChildNodes)) throw new UnityException("CharacterChildNodes component not found");
    }

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
                if (_rigidBodyComponent.linearVelocity != Vector2.zero)
                {
                    TryThrow(_rigidBodyComponent.linearVelocity.normalized);
                }
                else
                {
                    TryThrow(_characterVisualComponent.SpritesFlipped ? Vector2.left : Vector2.right, 0.25f);
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

    private void Update()
    {
        if (_currentHoldObject == null) return;
        if (_characterActionsComponent.CharacterAimingAction == null || !_characterActionsComponent.CharacterAimingAction.IsAbleToAim) return;

        float aimDelta = _characterActionsComponent.CharacterAimingAction.AimSpeed * Time.deltaTime;
        Vector2 currentAim = _characterActionsComponent.CharacterAimingAction.GetCurrentAimNormalized();

        //setting current holded object's rotation
        Quaternion targetAngle = new();
        targetAngle.eulerAngles = new Vector3(
            0f,
            math.lerp(
                _currentHoldObject.transform.rotation.eulerAngles.y,
                currentAim.x < 0f ? 180f : 0f,
                aimDelta
                ),
            currentAim.y * 90f
            );

        _currentHoldObject.transform.rotation = targetAngle;

        //setting current holded object's location
        Vector2 holdObjectPositionXY = Vector2.Lerp(
            _currentHoldObject.transform.position,
            VectorMath.Vec3ToVec2(_characterChildNodes.Center.transform.position) + currentAim * _currentHoldObject.HoldDistanceWhenIsHolded,
            aimDelta
            );

        _currentHoldObject.transform.position = new Vector3(
            holdObjectPositionXY.x,
            holdObjectPositionXY.y,
            _characterChildNodes.Center.transform.position.z
            );
    }

    public bool TryThrow(Vector2 align, float throwForceMultiplier = 1f)
    {
        if (_currentHoldObject == null) return false;

        _currentHoldObject.CurrentHolder = null;
        _currentHoldObject.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform;

        _currentHoldObject.transform.rotation.Set(0f, 0f, _currentHoldObject.transform.rotation.z, _currentHoldObject.transform.rotation.w);

        if (_currentHoldObject.TryGetComponent(out Rigidbody2D holdObjectRigidBody))
        {
            holdObjectRigidBody.linearVelocity = align * ThrowForce * throwForceMultiplier * _currentHoldObject.ThrowForceMultiplier;
            holdObjectRigidBody.angularVelocity = _currentHoldObject.ThrowRotationForce * (_characterVisualComponent.SpritesFlipped ? -1f : 1f);
        }

        _lastHoldObject = _currentHoldObject;
        _currentHoldObject = null;

        return true;
    }

    public bool TryGrab(Holdable holdable)
    {
        if (
            _isAbleToGrabObjects &&
            _currentHoldObject != null &&
            Vector3.Distance(holdable.transform.position, transform.position) > _characterActionsComponent.CharacterInteractAction.InteractRange * MaxGrabRangeMultiplier
            )
        {
            return false;
        }

        _currentHoldObject = holdable;

        _currentHoldObject.CurrentHolder = this;
        _currentHoldObject.transform.parent = transform;

        return true;
    }
}
