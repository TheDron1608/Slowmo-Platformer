using System;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Holdable : Interactable
{
    public bool RotatableWhenIsHolded = true;
    public float HoldDistanceWhenIsHolded = 0.75f;
    public float ThrowForceMultiplier = 1.0f;
    public float ThrowRotationForce = 12.5f;

    private CharacterHoldingObjects _currentHolder = null;
    private CharacterHoldingObjects _lastHolder = null;

    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _colliderComponent;

    protected override void Awake()
    {
        base.Awake();

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

            _rigidBodyComponent.simulated = _currentHolder == null;
        }
    }

    public CharacterHoldingObjects LastHolder
    {
        get => _lastHolder;
        private set => _lastHolder = value;
    }

    public bool TryGive(CharacterHoldingObjects newHolder)
    {
        return newHolder.TryGrab(this);
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterHoldingObjects charHoldingObjects))
        {
            charHoldingObjects.TryGrab(this);
        }
    }
}
