using System;
using UnityEngine;

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterHookProjectile : MonoBehaviour
{
    const float STUCK_OFFSET = 0.0625f * 6;

    public Transform HookTailConnection;

    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _colliderComponent;
    private CharacterHook _hook;
    private Vector3 _positionPrevFrame = Vector3.zero;
    private Vector2 _velocityPrevFrame = Vector2.zero;

    public Rigidbody2D RigidBodyComponent
    {
        get => _rigidBodyComponent;
    }

    public bool IsStuck
    {
        get => _rigidBodyComponent.bodyType == RigidbodyType2D.Static;
        set => _rigidBodyComponent.bodyType = value ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
    }

    private void Awake()
    {
        _rigidBodyComponent = GetComponent<Rigidbody2D>() ?? throw new UnityException("RigidBody2D component not found");
        _colliderComponent = GetComponent<Collider2D>() ?? throw new UnityException("Collider2D component not found");
        _hook = transform.parent.GetComponent<CharacterHook>() ?? throw new UnityException("CharacterHook component not found at " + transform.parent.name);
    }

    private void Update()
    {
        if (!IsStuck)
        {
            transform.rotation = VectorMath.Vec2ToQuarterninon2D(_rigidBodyComponent.linearVelocity);
        }
    }
    
    private void LateUpdate()
    {
        _positionPrevFrame = transform.position;
        _velocityPrevFrame = _rigidBodyComponent.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsStuck) return;
        IsStuck = true;

        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _velocityPrevFrame, 0.5f, 1 << layer.EnviromentLayer);
        if (hit.point != Vector2.zero)
        {
            transform.position = VectorMath.Vec2ToVec3(hit.point + hit.normal * STUCK_OFFSET, transform.position.z);
            transform.rotation = VectorMath.Vec2ToQuarterninon2D(-hit.normal);
        }
    }
}