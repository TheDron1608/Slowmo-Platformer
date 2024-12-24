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

    public event EventHandler<Holdable> OnHoldableChanged;

    private void Awake()
    {
        if (!TryGetComponent(out _characterInteractWithObjectsComponent)) throw new UnityException("CharacterInteractWithObjects component not found");
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
                TryThrow(Vector2.zero, 0.25f);
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
            holdObjectRigidBody.linearVelocity = align * ThrowForce * throwForceMultiplier;
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
