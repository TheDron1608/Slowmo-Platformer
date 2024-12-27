using System;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CharacterHoldingObjects;
using UnityEngine.UIElements;

public class Holdable : Interactable
{
    const int ON_GRAB_SORTING_ORDER_ADD = 50;

    public class OnThrownEventArgs
    {
        public OnThrownEventArgs(CharacterHoldingObjects thrower, Vector2 direction)
        {
            Thrower = thrower;
            Direction = direction;
        }
        public CharacterHoldingObjects Thrower;
        public Vector2 Direction;
    }

    public bool RotatableWhenIsHolded = true;
    public bool ResetRotationWhenIsHolded = false;
    public float HoldDistanceWhenIsHolded = 0.75f;
    public float ThrowForceMultiplier = 1.0f;
    public float ThrowRotationForce = 12.5f;

    private CharacterHoldingObjects _currentHolder = null;
    private CharacterHoldingObjects _lastHolder = null;

    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _colliderComponent;

    public event EventHandler<CharacterHoldingObjects> OnGiven;
    public event EventHandler<OnThrownEventArgs> OnThrown;

    private void Awake()
    {
        OnAwake();
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("Collider2D component not found");
    }

    public CharacterHoldingObjects CurrentHolder
    {
        get => _currentHolder;
        set
        {
            if (_currentHolder != null )
            {
                _lastHolder = _currentHolder;
            }
            _currentHolder = value;

            _rigidBodyComponent.simulated = _currentHolder is null;
        }
    }

    public CharacterHoldingObjects LastHolder
    {
        get => _lastHolder;
        private set => _lastHolder = value;
    }

    public void Give(CharacterHoldingObjects newHolder)
    {
        newHolder.CurrentHoldObject = this;

        CurrentHolder = newHolder;
        transform.parent = newHolder.transform;
        if (ResetRotationWhenIsHolded)
        {
            Quaternion baseRotation = new();
            baseRotation.eulerAngles = Vector3.zero;
            transform.rotation = baseRotation;
        }
        _spriteRendererComponent.sortingOrder += ON_GRAB_SORTING_ORDER_ADD;

        OnGiven?.Invoke(this, newHolder);
        OnPickedUp();
    }

    public void Throw(Vector2 direction, float throwForceMultiplier = 1f)
    {
        if (CurrentHolder == null) return;

        CurrentHolder.CurrentHoldObject = null;
        transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform;
        _spriteRendererComponent.sortingOrder -= ON_GRAB_SORTING_ORDER_ADD;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new Vector3(0f, 0f, direction.y * 90f);
        transform.rotation = newRotation;

        _rigidBodyComponent.linearVelocity = direction * CurrentHolder.ThrowForce * throwForceMultiplier * ThrowForceMultiplier;
        if (CurrentHolder.TryGetComponent(out CharacterVisual characterVisual))
        {
            _rigidBodyComponent.angularVelocity = ThrowRotationForce * (characterVisual.SpritesFlipped ? -1f : 1f);
        }
        else
        {
            _rigidBodyComponent.angularVelocity = ThrowRotationForce;
        }

        CurrentHolder.CurrentHoldObject = null;
        CurrentHolder = null;

        OnThrown?.Invoke(this, new OnThrownEventArgs(CurrentHolder, direction));
        OnThrow();
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterHoldingObjects charHoldingObjects))
        {
            charHoldingObjects.TryGrab(this);
        }
    }

    protected virtual void OnThrow()
    {

    }

    protected virtual void OnPickedUp()
    {

    }
}
