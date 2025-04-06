using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsParticle : MonoBehaviour
{
    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private bool _enabledPhysics = true;

    public bool EnabledPhysics
    {
        get => _enabledPhysics;
        set
        {
            _rigidBodyComponent.simulated = value;
            _enabledPhysics = value;
        }
    }

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _collider)) throw new UnityException("Collider2D component not found");
        if (!TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found");

        LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForGameObject(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_enabledPhysics)
        {
            if (
                _rigidBodyComponent.linearVelocity == Vector2.zero && 
                collision.gameObject.TryGetComponent(out Rigidbody2D collisionRigidBody) &&
                (
                    collisionRigidBody.bodyType != RigidbodyType2D.Dynamic ||
                    !collisionRigidBody.simulated
                )
                )
            {
                _rigidBodyComponent.simulated = false;
                _enabledPhysics = false;
            }
        }
    }
}
