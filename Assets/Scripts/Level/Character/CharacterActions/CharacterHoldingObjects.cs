using System;
using UnityEngine;

public class CharacterHoldingObjects : MonoBehaviour
{
    public float ThrowForce = 10f;
    public float MaxGrabRangeMultiplier = 1f;

    private Holdable _currentHoldObject = null;
    private Holdable _lastHoldObject = null;
    private bool _isAbleToGrabObjects;

    private CharacterInteractWithObjects _characterInteractWithObjectsComponent;
    private Rigidbody2D _rigidBodyComponent;
    private CharacterVisual _characterVisualComponent;

    public event EventHandler<Holdable> OnHoldableChanged;

    private void Awake()
    {
        if (!TryGetComponent(out _characterInteractWithObjectsComponent)) throw new UnityException("CharacterInteractWithObjects component not found");
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _characterVisualComponent)) throw new UnityException("CharacterVisual component not found");
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

    public bool TryThrow(Vector2 align, float throwForceMultiplier = 1f)
    {
        if (_currentHoldObject == null) return false;

        _currentHoldObject.CurrentHolder = null;
        _currentHoldObject.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform;

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
            Vector3.Distance(holdable.transform.position, transform.position) > _characterInteractWithObjectsComponent.InteractRange * MaxGrabRangeMultiplier
            )
        {
            return false;
        }

        _currentHoldObject = holdable;

        _currentHoldObject.CurrentHolder = this;
        _currentHoldObject.transform.parent = transform;
        _currentHoldObject.transform.localPosition = Vector3.zero;

        return true;
    }
}
