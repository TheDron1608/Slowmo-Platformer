using UnityEngine;

public class PhysicsParticle : AbstractSpriteParticle, IStuckableObject
{
    protected Rigidbody2D _rigidBodyComponent;
    private bool _enabledPhysics = true;
    private Collider2D _stuckedToCollider = null;

    public bool EnabledPhysics
    {
        get => _enabledPhysics;
        set
        {
            _rigidBodyComponent.simulated = value;
            _enabledPhysics = value;
        }
    }

    public Collider2D StuckedToCollider 
    { 
        get => _stuckedToCollider; 
        set
        {
            if (value != null)
            {
                if (value.TryGetComponent(out IStuckToObject stuckToObject))
                {
                    _stuckedToCollider = value;
                    stuckToObject.AddStuckedObject(this);
                }
                else if (value.TryGetComponent(out AbstractCharacterComponent charComponent))
                {
                    _stuckedToCollider = charComponent.CharComponents.CharacterRigidBodyCapsuleCollider;
                    charComponent.CharComponents.CharacterStuckedObjects.AddStuckedObject(this);
                }
                else
                {
                    _stuckedToCollider = value;
                }
            }
            else
            {
                _stuckedToCollider = null;
            }

            EnabledPhysics = _stuckedToCollider == null;
        }
    }

    public override void SetParticleAttrs(
        AbstractParticle original,
        Vector2 position,
        Vector2 direction,
        float angle,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer,
        bool enablePhysics = true
        )
    {
        base.SetParticleAttrs(original, position, direction, angle, velocity, angularVelocity, material, layer);

        BoxCollider2D colliderComponent = GetComponent<BoxCollider2D>();
        BoxCollider2D originalColliderComponent = original.GetComponent<BoxCollider2D>();
        colliderComponent.size = originalColliderComponent.size;
        colliderComponent.offset = originalColliderComponent.offset;

        SoundPlayerOnCollide collideSoundComponent = GetComponent<SoundPlayerOnCollide>();
        SoundPlayerOnCollide originalCollideSoundComponent = original.GetComponent<SoundPlayerOnCollide>();
        collideSoundComponent.VeclocityForMaxVolume = originalCollideSoundComponent.VeclocityForMaxVolume;
        collideSoundComponent.SoundPlayer.DefaultSound = originalCollideSoundComponent.SoundPlayer.DefaultSound;
        collideSoundComponent.SoundPlayer.Volume = originalCollideSoundComponent.SoundPlayer.Volume;
        collideSoundComponent.SoundPlayer.Pitch = originalCollideSoundComponent.SoundPlayer.Pitch;
        collideSoundComponent.SoundPlayer.DynamicVolumeMultiplier = 1f;

        _rigidBodyComponent.linearVelocity = direction * velocity;
        _rigidBodyComponent.angularVelocity = angularVelocity;
        StuckedToCollider = null;
        EnabledPhysics = enablePhysics;
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