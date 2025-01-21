using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;

public class Holdable : Interactable
{
    const int ON_GRAB_SORTING_ORDER_ADD = 50;
    const float STUCK_IN_WALL_STRINGHT = 40f;

    public class OnThrownEventArgs
    {
        public OnThrownEventArgs(CharacterHoldingObjects thrower, Vector2 direction)
        {
            Thrower = thrower;
            Direction = direction;
        }
        public CharacterHoldingObjects Thrower;
        public Vector2 Direction;
    }

    [Header("Holdable")]
    public bool RotatableWhenIsHolded = true;
    public bool ResetRotationWhenIsHolded = false;
    public float HoldDistanceWhenIsHolded = 0.75f;
    public float ThrowForceMultiplier = 1.0f;
    public float ThrowRotationForce = 12.5f;
    public float SpeedToGetThroughWall = 15f;

    private CharacterHoldingObjects _currentHolder = null;
    private CharacterHoldingObjects _lastHolder = null;

    private Rigidbody2D _rigidBodyComponent;
    private Collider2D _colliderComponent;
    private Collider2D _stuckedToCollider = null;

    private bool _isStuck = false;
    private Coroutine _stuckCoroutine = null;

    public event EventHandler<CharacterHoldingObjects> OnGiven;
    public event EventHandler<OnThrownEventArgs> OnThrown;

    public Collider2D StuckedToCollider
    {
        get => _stuckedToCollider;
        private set => _stuckedToCollider = value;
    }

    private void Awake()
    {
        OnAwake();
    }
    private void Update()
    {
        OnUpdate();
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("Collider2D component not found");
    }

    protected virtual void OnUpdate()
    {
        if (VectorMath.RigidBodyVelocityToSpeed(_rigidBodyComponent) > SpeedToGetThroughWall)
        {
            _colliderComponent.isTrigger = true;
        }
        else if (!_isStuck)
        {
            _colliderComponent.isTrigger = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _isStuck = true;
        _stuckCoroutine = StartCoroutine(StuckCoroutine(collision));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _isStuck = false;
        if (_stuckCoroutine != null)
        {
            StopCoroutine(_stuckCoroutine);
        }
    }

    private IEnumerator StuckCoroutine(Collider2D stuckWho)
    {
        while (VectorMath.RigidBodyVelocityToSpeed(_rigidBodyComponent) > 0.5f)
        {
            _rigidBodyComponent.linearVelocity = math.lerp(_rigidBodyComponent.linearVelocity, Vector2.zero, Time.fixedDeltaTime * STUCK_IN_WALL_STRINGHT);
            yield return new WaitForFixedUpdate();
        }
        if (
            stuckWho.TryGetComponent(out Rigidbody2D stuckWhoRigidBody) && 
            (
                stuckWhoRigidBody.bodyType == RigidbodyType2D.Static ||
                stuckWhoRigidBody.bodyType == RigidbodyType2D.Kinematic
            )
            )
        {
            _rigidBodyComponent.bodyType = RigidbodyType2D.Static;

            StuckedToCollider = stuckWho;
            if (StuckedToCollider.TryGetComponent(out Holdable stuckWhoHoldable))
            {
                stuckWhoHoldable.OnGiven += StuckedObject_OnGiven;
            }
        }
    }

    private void StuckedObject_OnGiven(object sender, CharacterHoldingObjects e)
    {
        if (StuckedToCollider.TryGetComponent(out Holdable stuckWhoHoldable))
        {
            stuckWhoHoldable.OnGiven -= StuckedObject_OnGiven;
        }
        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
        _isStuck = false;
    }

    public CharacterHoldingObjects CurrentHolder
    {
        get => _currentHolder;
        set
        {
            if (_currentHolder != null )
            {
                _lastHolder = _currentHolder;
            }
            _currentHolder = value;

            _rigidBodyComponent.simulated = _currentHolder is null;
        }
    }

    public CharacterHoldingObjects LastHolder
    {
        get => _lastHolder;
        private set => _lastHolder = value;
    }

    public void Give(CharacterHoldingObjects newHolder)
    {
        OnGiven?.Invoke(this, newHolder);
        OnPickedUp(newHolder);
    }

    public void Throw(Vector2 direction, float throwForceMultiplier = 1f)
    {
        OnThrown?.Invoke(this, new OnThrownEventArgs(CurrentHolder, direction));
        OnThrow(direction, throwForceMultiplier);
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterHoldingObjects charHoldingObjects))
        {
            charHoldingObjects.TryGrab(this);
        }
    }

    protected virtual void OnThrow(Vector2 direction, float throwForceMultiplier = 1f)
    {
        CurrentHolder.CurrentHoldObject = null;
        transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform;
        _spriteRendererComponent.sortingOrder -= ON_GRAB_SORTING_ORDER_ADD;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new Vector3(0f, direction.x < 0f ? 180f : 0f, direction.y * 90f);
        transform.rotation = newRotation;

        _rigidBodyComponent.linearVelocity = direction * CurrentHolder.ThrowForce * throwForceMultiplier * ThrowForceMultiplier;
        if (CurrentHolder.TryGetComponent(out CharacterVisual characterVisual))
        {
            _rigidBodyComponent.angularVelocity = ThrowRotationForce * (characterVisual.SpritesFlipped ? -1f : 1f);
        }
        else
        {
            _rigidBodyComponent.angularVelocity = ThrowRotationForce;
        }

        CurrentHolder.CurrentHoldObject = null;
        CurrentHolder = null;

        //logic for weapon component and weapon class children classes
        if (TryGetComponent(out Weapon weapon)) 
        {
            weapon.IsThrown = true;
        }
        if (TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SetReloadSpeed(1f);
        }
    }

    protected virtual void OnPickedUp(CharacterHoldingObjects newHolder)
    {
        newHolder.CurrentHoldObject = this;

        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
        _colliderComponent.isTrigger = false;
        CurrentHolder = newHolder;
        transform.parent = newHolder.transform;
        if (ResetRotationWhenIsHolded)
        {
            Quaternion baseRotation = new();
            baseRotation.eulerAngles = Vector3.zero;
            transform.rotation = baseRotation;
        }
        _spriteRendererComponent.sortingOrder += ON_GRAB_SORTING_ORDER_ADD;

        //logic for weapon component and weapon class children classes
        if (TryGetComponent(out Weapon weapon))
        {
            weapon.AttackCooldown = 0f;
            weapon.IsThrown = false;

            if (TryGetComponent(out RangedWeapon rangedWeapon) && CurrentHolder.TryGetComponent(out CharacterReloading holderReloading))
            {
                rangedWeapon.SetReloadSpeed(holderReloading.ReloadSpeed);
            }

            if (TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
            {
                if (magReloadingWeapon.Unloaded && magReloadingWeapon.Mags > 0)
                {
                    magReloadingWeapon.TryCloseMag();
                }
            }

            if (TryGetComponent(out BulletReloadingWeapon bulletReloadWeapon))
            {
                if (bulletReloadWeapon.Unloaded && bulletReloadWeapon.LoadedLivingAmmoLeft > 0)
                {
                    bulletReloadWeapon.TryCloseMag();
                }
            }
        }
    }
}
