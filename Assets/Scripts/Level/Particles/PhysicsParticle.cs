using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsParticle : MonoBehaviour
{
    private Rigidbody2D _rigidBodyComponent;

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
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");

        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);
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
