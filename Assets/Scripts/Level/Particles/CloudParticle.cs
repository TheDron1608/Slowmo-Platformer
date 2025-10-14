using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CloudParticle : AbstractParticle
{
    const float GRAVITY_OFFSET = 0.3f;

    private Vector2 _currentVelocity;

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

        _currentVelocity = direction * velocity;
    }

    private void Update()
    {
        transform.position += VectorMath.Vec2ToVec3(_currentVelocity * Time.deltaTime);
        _currentVelocity += Vector2.up * GRAVITY_OFFSET * Time.deltaTime;
    }

    public override void RemoveParticle()
    {
        base.RemoveParticle();

        transform.parent = ParticlesManager.Instance.UnusedCloudParticleContainer;
    }
}
