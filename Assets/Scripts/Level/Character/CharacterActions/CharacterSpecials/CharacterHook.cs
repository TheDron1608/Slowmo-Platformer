using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.VFX;
using Unity.Mathematics;

public class CharacterHook : AbstractCharacterSpecial
{
    const float HOOK_TAIL_SPRITE_UNITS = 1f;

    public float HookAttracktionVelocity = 1.5f;
    public float MaxHookDistanceAttracktionVelocity = 5f;
    public float ThrowVelocity = 10f;
    public float NoGraviryDuration = 2f;

    [SerializeField] private CharacterHookProjectile _hookProjectile;
    [SerializeField] private SpriteRenderer _hookTail;

    private bool _isHooking = false;
    private float _timeSinceHook = 0f;

    public bool IsHooking
    {
        get => _isHooking;
    }

    public bool TryHook(Vector2 direction)
    {
        if (IsHooking) return false;

        _timeSinceHook = 0f;

        _hookTail.gameObject.SetActive(true);
        _hookProjectile.gameObject.SetActive(true);
        _hookProjectile.transform.position = CharComponents.Center.transform.position;
        _hookProjectile.IsStuck = false;
        _hookProjectile.RigidBodyComponent.linearVelocity = direction.normalized * ThrowVelocity;

        _isHooking = true;

        return true;
    }

    public bool TryStopHook()
    {
        if (!IsHooking) return false;

        _hookTail.gameObject.SetActive(false);
        _hookProjectile.gameObject.SetActive(false);

        _isHooking = false;

        return true;
    }

    private void FixedUpdate()
    {
        if (IsHooking)
        {
            _timeSinceHook += Time.fixedDeltaTime;

            //_hookProjectile.RigidBodyComponent.gravityScale = _timeSinceHook < NoGraviryDuration ? 0f : 1f;

            if (_hookProjectile.IsStuck)
            {
                Vector2 hookalign = (_hookProjectile.transform.position - CharComponents.Center.transform.position).normalized;
                float hookDistance = Vector2.Distance(_hookProjectile.transform.position, CharComponents.Center.transform.position);
                CharComponents.CharacterRigidBody.linearVelocity +=  hookalign * math.min(hookDistance, MaxHookDistanceAttracktionVelocity) * HookAttracktionVelocity;
            }
        }
    }

    private void Update()
    {
        Vector2 from = CharComponents.Center.transform.position;
        Vector2 to = _hookProjectile.HookTailConnection.position;
        float distance = Vector2.Distance(from, to) / HOOK_TAIL_SPRITE_UNITS;
        Vector2 targetRotation = (from - to).normalized;

        _hookTail.transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(targetRotation);
        _hookTail.transform.position = VectorMath.Vec2ToVec3(to + (targetRotation * distance / 2f), _hookTail.transform.position.z);
        _hookTail.size = new Vector2(distance, 1f);
    }
}