using UnityEngine;

public class BulletProjectile : Projectile
{
    public float BulletSpeed = 35f;

    private Vector2 _moveAlign;

    public Vector2 MoveAlign
    {
        get => _moveAlign;
        set
        {
            _moveAlign = value.normalized;
        }
    }

    private void Start()
    {
        float deltaRange = BulletSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x - deltaRange * MoveAlign.x,
            transform.position.y - deltaRange * MoveAlign.y,
            transform.position.z
            );
    }

    private void Update()
    {
        float deltaRange = BulletSpeed * Time.deltaTime;
        transform.position = new Vector3(
            transform.position.x + deltaRange * MoveAlign.x,
            transform.position.y + deltaRange * MoveAlign.y,
            transform.position.z
            );
    }
}
