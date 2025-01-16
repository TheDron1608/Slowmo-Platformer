using UnityEngine;

public class BulletProjectile : Projectile
{
    const float MAX_RANGE_RADOMIZED_EXTRA_VALUE = 1.5f;

    public float BulletSpeed = 35f;
    public float MaxRange = 350f;

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;

    private float _rangeMoved = 0f;

    public Quaternion MoveAlign
    {
        get => _moveAlign;
        set
        {
            transform.rotation = value;
            _moveAlign = value.normalized;
            _moveAlignVec2 = VectorMath.Quartenion2DToVec2(_moveAlign);
        }
    }

    private void Start()
    {
        float deltaRange = BulletSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x - deltaRange * _moveAlignVec2.x,
            transform.position.y - deltaRange * _moveAlignVec2.y,
            transform.position.z
            );
    }

    private void Update()
    {
        float deltaRange = BulletSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + deltaRange * _moveAlignVec2.x,
            transform.position.y + deltaRange * _moveAlignVec2.y,
            transform.position.z
            );

        _rangeMoved += deltaRange;
        if (_rangeMoved > MaxRange )
        {
            Remove();
        }
    }

    public override void InitializeOwner(Weapon owner)
    {
        base.InitializeOwner(owner);

        MaxRange = Weapon.GetComponent<RangedWeapon>().MaxRange + (Random.value - 0.5f) * MAX_RANGE_RADOMIZED_EXTRA_VALUE;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Remove();
    }
}
