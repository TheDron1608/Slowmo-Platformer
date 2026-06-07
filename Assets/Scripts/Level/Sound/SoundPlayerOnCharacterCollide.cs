using UnityEngine;

public class SoundPlayerOnCharacterCollide : AbstractCharacterComponent
{
    [SerializeField] private AbstractSoundPlayer _soundPlayer;
    public float VeclocityForMaxVolume = 15f;
    public Sound DefaultCollideSound;
    public Sound StunnedCollideSound;

    private Vector2 _velocityPrevFrame = Vector2.zero;


    private void FixedUpdate()
    {
        _velocityPrevFrame = CharComponents.CharacterRigidBody.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _soundPlayer.DynamicVolumeMultiplier = 
            NumberMath.LimitFloatBetweenZeroAndOne(
                (_velocityPrevFrame - (collision.rigidbody.bodyType == RigidbodyType2D.Dynamic ? collision.rigidbody.linearVelocity : Vector2.zero))
                .magnitude / VeclocityForMaxVolume
                );

        _soundPlayer.PlaySound(CharComponents.CharacterEffectsReceiver.GetHasEffect<AbstractStun>() ? StunnedCollideSound : DefaultCollideSound);
    }
}