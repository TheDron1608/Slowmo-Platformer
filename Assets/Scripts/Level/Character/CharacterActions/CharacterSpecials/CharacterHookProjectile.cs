using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterHookProjectile : MonoBehaviour, IStuckableObject
{
    const float STUCK_OFFSET = 0.0625f * 6;

    public Transform HookTailConnection;
    public List<AbstractEffect> EffectsOnStuck = new();

    [SerializeField] private SoundPlayer _soundOnStuck;
    [SerializeField] private SoundPlayer _soundOnStuckInCharacter;

    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _colliderComponent;
    private CharacterHook _hook;
    private Vector2 _velocityPrevFrame = Vector2.zero;
    private Collider2D _stuckedToCollider = null;
    private AbstractCharacterComponent _stuckToCharacter = null;

    public Rigidbody2D RigidBodyComponent
    {
        get => _rigidBodyComponent;
    }

    public bool IsStuck
    {
        get => !_rigidBodyComponent.simulated;
        set => _rigidBodyComponent.simulated = !value;
    }

    public Collider2D StuckedToCollider
    {
        get => _stuckedToCollider;
        set
        {
            if (_stuckedToCollider == value) return;

            if (!_stuckedToCollider.IsDestroyed() && (_stuckedToCollider?.TryGetComponent(out IStuckToObject oldStuckTo) ?? false))
            {
                oldStuckTo.RemoveStuckedObject(this);
            }

            if (value == null)
            {
                _stuckToCharacter = null;
                transform.parent = _hook.transform;
                _stuckedToCollider = null;
            }
            else if (value.TryGetComponent(out IStuckToObject stuckToObject))
            {
                _stuckToCharacter = null;
                _stuckedToCollider = value;
                stuckToObject.AddStuckedObject(this);
                _soundOnStuck.PlaySound();
            }
            else if (value.TryGetComponent(out AbstractCharacterComponent charComponent))
            {
                if (charComponent.CharComponents != _hook.CharComponents)
                {
                    _stuckToCharacter = charComponent;
                    _stuckedToCollider = charComponent.CharComponents.CharacterRigidBodyCapsuleCollider;
                    charComponent.CharComponents.CharacterStuckedObjects.AddStuckedObject(this);
                    _soundOnStuckInCharacter.PlaySound();
                }
                else
                {
                    transform.parent = _hook.transform;
                    _stuckToCharacter = null;
                    _stuckedToCollider = null;
                }
            }
            else
            {
                _stuckToCharacter = null;
                _stuckedToCollider = value;

                ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, _velocityPrevFrame, 0.5f, 1 << layer.EnviromentLayer);
                if (hit.point != Vector2.zero)
                {
                    transform.position = VectorMath.Vec2ToVec3(hit.point + hit.normal * STUCK_OFFSET, transform.position.z);
                    transform.rotation = VectorMath.Vec2ToQuarterninon2D(-hit.normal);
                }
                transform.parent = _hook.CharComponents.CharacterCollision.CurrentZLayer.ProjectilesContainer;
                _soundOnStuck.PlaySound();
            }

            if (_stuckToCharacter != null)
            {
                _stuckToCharacter.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnStuck, _hook.CharComponents.CharacterAttacking);
            }
            else if (_stuckedToCollider?.TryGetComponent(out ObjectEffectsReceiver oldEffectsReceiver) ?? false)
            {
                oldEffectsReceiver.ApplyEffect(EffectsOnStuck, _hook.CharComponents.CharacterAttacking);
            }

            IsStuck = _stuckedToCollider != null;
        }
    }

    public AbstractCharacterComponent StuckToCharacter
    {
        get => _stuckToCharacter;
    }

    private void Awake()
    {
        _rigidBodyComponent = GetComponent<Rigidbody2D>() ?? throw new UnityException("RigidBody2D component not found");
        _colliderComponent = GetComponent<Collider2D>() ?? throw new UnityException("Collider2D component not found");
        _hook = transform.parent.GetComponent<CharacterHook>() ?? throw new UnityException("CharacterHook component not found at " + transform.parent.name);


        Physics2D.IgnoreCollision(_hook.CharComponents.CharacterRigidBodyCapsuleCollider, _colliderComponent);
        foreach (CharacterLimbPart limbPart in _hook.CharComponents.CharacterPartsManager.GetAllLimbs())
        {
            Physics2D.IgnoreCollision(limbPart.Collider, _colliderComponent);
        }
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
        _velocityPrevFrame = _rigidBodyComponent.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsStuck)
        {
            StuckedToCollider = collision.collider;
        }
    }
}