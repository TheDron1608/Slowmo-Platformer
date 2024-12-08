using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    private Rigidbody2D _rigidBodyComponent;
    private CapsuleCollider2D _capsuleColliderComponent;

    private float _timeInAir;
    private float _timeOnGround;
    private bool _wasGroundedPrevFrame = true;


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

    public bool IsCollidingFloor()
    {
        Vector2 rayCastHitOrigin = new Vector2(transform.position.x, transform.position.y) + _capsuleColliderComponent.offset;
        RaycastHit2D rayCastHit = Physics2D.Raycast(rayCastHitOrigin, Vector2.down, 99999f , LayerMask.NameToLayer("CharactersColliders"));
        return rayCastHit.collider != null;
    }



    private void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent<CapsuleCollider2D>(out _capsuleColliderComponent)) throw new UnityException("CapsuleCollider2D component not found");
    }

    private void Update()
    {
        UpdateTimeOnAirOrGround();
        Debug.Log(IsCollidingFloor());
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

    private void LateUpdate()
    {
        _wasGroundedPrevFrame = _rigidBodyComponent.linearVelocityY == 0f;
    }
}
