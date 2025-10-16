using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CloudParticle : AbstractParticle
{
    const float GRAVITY_OFFSET = 0.3f;
    const string ANIMATOR_RESET_TRIGGER_NAME = "Reset";

    private Vector2 _currentVelocity;

    public override void SetParticleAttrs(
        AbstractParticle original,
        Vector2 position,
        Vector2 direction,
        float angle,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer
        )
    {
        base.SetParticleAttrs(original, position, direction, angle, velocity, angularVelocity, material, layer);

        Animator animator = gameObject.GetComponent<Animator>();
        Animator originalAnimator = original.GetComponent<Animator>();
        animator.runtimeAnimatorController = originalAnimator.runtimeAnimatorController;
        animator.SetTrigger(ANIMATOR_RESET_TRIGGER_NAME);

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
