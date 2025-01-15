using UnityEngine;

public class BulletProjectile : Projectile
{
    public float BulletSpeed = 35f;

    private Quaternion _moveAlign;
    private Vector2 _moveAlignVec2;

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
    }
}
