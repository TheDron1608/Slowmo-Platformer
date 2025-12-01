using UnityEngine;

[RequireComponent(typeof(SoundPlayer))]
public class SoundPlayerOnCollide : MonoBehaviour
{
    const float MASS_VOLUME_AFFECTION_MULTIPLIER = 0.1f;

    [SerializeField] private SoundPlayer _soundPlayer;
    public float VeclocityForMaxVolume = 15f;

    private Rigidbody2D _rigidBodyComponent;
    private Vector2 _velocityPrevFrame = Vector2.zero;

    private void Awake()
    {
        _rigidBodyComponent = GetComponentInParent<Rigidbody2D>() ?? throw new UnityException("not found RigidBody2D component in " + transform.parent.name);
    }

    private void FixedUpdate()
    {
        _velocityPrevFrame = _rigidBodyComponent.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _soundPlayer.Volume = 
            NumberMath.LimitFloatBetweenZeroAndOne(
                (_velocityPrevFrame - (collision.rigidbody.bodyType == RigidbodyType2D.Dynamic ? collision.rigidbody.linearVelocity : Vector2.zero))
                .magnitude / VeclocityForMaxVolume * (1f - _rigidBodyComponent.mass * MASS_VOLUME_AFFECTION_MULTIPLIER)
                );
        _soundPlayer.PlaySound();
    }
}