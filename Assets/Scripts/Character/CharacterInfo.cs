using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    private Rigidbody2D _rigidBodyComponent;

    private float _timeInAir;
    private float _timeOnGround;

    public float TimeInAir
    {
        get => _timeInAir;
        private set => _timeInAir = value;
    }
    public float TimeOnGround
    {
        get => _timeOnGround;
        private set => _timeOnGround = value;
    }

    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
    }

    private void Update()
    {
        UpdateTimeOnAirOrGround();
    }

    private void UpdateTimeOnAirOrGround()
    {
        if (_rigidBodyComponent.linearVelocityY == 0f)
        {
            _timeOnGround += Time.deltaTime;
            _timeInAir = 0f;
        }
        else
        {
            _timeInAir += Time.deltaTime;
            _timeOnGround = 0f;
        }
    }
}
