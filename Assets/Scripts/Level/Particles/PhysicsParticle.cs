using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class PhysicsParticle : AbstractParticle
{
    protected Rigidbody2D _rigidBodyComponent;
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

    public override void SetParticleAttrs(
        Vector2 position,
        Vector2 direction,
        float angle,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer,
        Sprite sprite = null,
        Animator animator = null,
        BoxCollider2D collider = null,
        string particleName = "untitled"
        )
    {
        base.SetParticleAttrs(position, direction, angle, velocity, angularVelocity, material, layer, sprite, animator, collider, particleName);

        _rigidBodyComponent.linearVelocity = direction * velocity;
        _rigidBodyComponent.angularVelocity = angularVelocity;
        EnabledPhysics = true;
    }


    public override void RemoveParticle()
    {
        base.RemoveParticle();

        EnabledPhysics = false;
        transform.parent = ParticlesManager.Instance.UnusedPhysicsParticleContainer;
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
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