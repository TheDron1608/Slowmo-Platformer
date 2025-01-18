using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsParticle : MonoBehaviour
{
    const int MAX_PARTICLES = 128;
    const float MAX_SIMULATED_PARTICLE_LIFE_SECONDS = 15f;
    const float PARTICLE_DISSAPEAR_TIME_SECONDS = 2f;

    public static List<PhysicsParticle> ParticlesOnLevel = new();

    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private Coroutine _removeWhenMaxParticleLifeIsOutCoroutine;
    private bool _enabledPhysics = true;

    private void Awake()
    {
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _collider)) throw new UnityException("Collider2D component not found");
        if (!TryGetComponent(out _spriteRenderer)) throw new UnityException("SpriteRenderer component not found");

        LayerManager.Instance.GetZLayerOfGameObject(gameObject).UpdateLayerForGameObject(gameObject);

        ParticlesOnLevel.Add(this);
        if (ParticlesOnLevel.Count > MAX_PARTICLES)
        {
            ParticlesOnLevel[0].RemoveParticle();
            ParticlesOnLevel.RemoveAt(0);
        }

        _removeWhenMaxParticleLifeIsOutCoroutine = StartCoroutine(RemoveWhenMaxParticleLifeIsOutCoroutine());
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
                StopCoroutine(_removeWhenMaxParticleLifeIsOutCoroutine);
            }
        }
    }

    public void RemoveParticle()
    {
        StartCoroutine(RemoveParticleProcess());
    }

    private IEnumerator RemoveParticleProcess()
    {
        while (_spriteRenderer.color.a > 0f)
        {
            _spriteRenderer.color = new Color(
                _spriteRenderer.color.r,
                _spriteRenderer.color.g,
                _spriteRenderer.color.b,
                _spriteRenderer.color.a - Time.deltaTime
                );
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    private IEnumerator RemoveWhenMaxParticleLifeIsOutCoroutine()
    {
        yield return new WaitForSeconds(MAX_SIMULATED_PARTICLE_LIFE_SECONDS);
        RemoveParticle();
    }
}
