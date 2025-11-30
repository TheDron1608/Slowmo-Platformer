using UnityEngine;

[RequireComponent(typeof(SoundPlayer))]
public class SoundPlayerOnCollide : MonoBehaviour
{
    public float VeclocityForMaxVolume = 15f;

    private Rigidbody2D _rigidBodyComponent;
    private SoundPlayer _soundPlayerComponent;

    private void Awake()
    {
        _rigidBodyComponent = GetComponentInParent<Rigidbody2D>() ?? throw new UnityException("not found RigidBody2D component in " + transform.parent.name);
        _soundPlayerComponent = GetComponent<SoundPlayer>() ?? throw new UnityException("not found SoundPlayer component in " + transform.name);
    }

    private void LateUpdate()
    {
        _soundPlayerComponent.Volume = NumberMath.LimitFloatBetweenZeroAndOne(_rigidBodyComponent.linearVelocity.magnitude / VeclocityForMaxVolume);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _soundPlayerComponent.PlaySound();
    }
}