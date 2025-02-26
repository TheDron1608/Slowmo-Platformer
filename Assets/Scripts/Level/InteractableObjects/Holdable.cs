using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

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
    public float SpeedToHitCharacter = 7.5f;
    public float SpeedToGetThrough = 15f;
    public List<AbstractCharacterEffect> EffectsOnThrowHit = new();

    private CharacterHoldingObjects _currentHolder = null;
    private CharacterHoldingObjects _lastHolder = null;

    private Rigidbody2D _rigidBodyComponent;
    private BoxCollider2D _colliderComponent;
    private CircleCollider2D _thrownColliderComponent;

    private Collider2D _stuckedToCollider = null;
    private float _velocitySpeedPreviousFrame = 0f;
    private bool _isStuck = false;

    public event EventHandler<CharacterHoldingObjects> OnGiven;
    public event EventHandler<OnThrownEventArgs> OnThrown;

    public Collider2D StuckedToCollider
    {
        get => _stuckedToCollider;

        set
        {
            if (_stuckedToCollider == value) return;

            if (value != null)
            {
                if (value.TryGetComponent(out AbstractCharacterComponent charComponent))
                {
                    _rigidBodyComponent.bodyType = RigidbodyType2D.Kinematic;
                    transform.parent = charComponent.CharComponents.transform;
                    charComponent.CharComponents.CharacterStuckedObjects.StuckedObjects.Add(this);
                }

                else if
                    (
                        value.TryGetComponent(out Rigidbody2D stuckWhoRigidBody) &&
                        _velocitySpeedPreviousFrame >= SpeedToGetThrough &&
                        (stuckWhoRigidBody.bodyType == RigidbodyType2D.Static || stuckWhoRigidBody.bodyType == RigidbodyType2D.Kinematic)
                    )
                {
                    _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
                }
                _isStuck = true;
            }

            else
            {
                _isStuck = false;
                if (_stuckedToCollider.TryGetComponent(out AbstractCharacterComponent charComponent))
                {
                    charComponent.CharComponents.CharacterStuckedObjects.StuckedObjects.Remove(this);
                }
                _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
            }

            _stuckedToCollider = value;
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("BoxCollider2D component not found");
        if (!TryGetComponent(out _thrownColliderComponent)) throw new UnityException("CircleCollider2D component not found");
    }

    private void Update()
    {
        if (_isStuck)
        {
            _rigidBodyComponent.excludeLayers = int.MaxValue; //excludes all layers
        }
        else if (CurrentHolder == null)
        {
            _rigidBodyComponent.excludeLayers = 0;

            if (_velocitySpeedPreviousFrame <= SpeedToGetThrough)
            {
                _colliderComponent.enabled = true;
                _thrownColliderComponent.enabled = false;
            }
            else
            {
                _colliderComponent.enabled = false;
                _thrownColliderComponent.enabled = true;
            }

            if (_velocitySpeedPreviousFrame <= SpeedToHitCharacter)
            {
                _rigidBodyComponent.includeLayers = _colliderComponent.includeLayers;
            }
            else
            {
                _rigidBodyComponent.includeLayers = _thrownColliderComponent.includeLayers;
            }
        }
    }

    private void FixedUpdate()
    {
        _velocitySpeedPreviousFrame = VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isStuck) return;
        if (collision.collider.TryGetComponent(out AbstractCharacterComponent charComponent) && charComponent.CharComponents.CharacterHolding == LastHolder) return;

        if (charComponent != null && _velocitySpeedPreviousFrame >= SpeedToHitCharacter)
        {
            charComponent.CharComponents.CharacterEffects.ApplyEffect(EffectsOnThrowHit, this);
        }

        if (_velocitySpeedPreviousFrame >= SpeedToGetThrough)
        {
            StuckedToCollider = collision.collider;
        }
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
        _isStuck = false;
        CurrentHolder.CurrentHoldObject = null;
        transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform;
        _spriteRendererComponent.sortingOrder -= ON_GRAB_SORTING_ORDER_ADD;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new Vector3(0f, direction.x < 0f ? 180f : 0f, direction.y * 90f);
        transform.rotation = newRotation;

        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
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
            for (int i = 0; i < weapon.Projectiles.Count; i++)
            {
                if (weapon.Projectiles[i] is MeleeProjectile)
                {
                    weapon.Projectiles[i].RemoveSelf();
                }
            }
        }
        if (TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SetReloadSpeed(1f);
        }
        if (TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon))
        {
            if (hammerWeapon.IsHammerring)
            {
                hammerWeapon.TrySetHammered(false);
            }
        }
        if (TryGetComponent(out Chainsaw chainsaw))
        {
            chainsaw.Started = false;
        }
    }

    protected virtual void OnPickedUp(CharacterHoldingObjects newHolder)
    {
        newHolder.CurrentHoldObject = this;
        _isStuck = false;

        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
        CurrentHolder = newHolder;
        transform.parent = newHolder.transform;
        if (ResetRotationWhenIsHolded)
        {
            Quaternion baseRotation = new();
            baseRotation.eulerAngles = Vector3.zero;
            transform.rotation = baseRotation;
        }
        _spriteRendererComponent.sortingOrder += ON_GRAB_SORTING_ORDER_ADD;
        StuckedToCollider = null;

        //logic for weapon component and weapon class children classes
        if (TryGetComponent(out Weapon weapon))
        {
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

            if (TryGetComponent(out SpinableMeleeWeapon spinableMeleeWeapon))
            {
                spinableMeleeWeapon.Spin();
            }
        }
    }
}
