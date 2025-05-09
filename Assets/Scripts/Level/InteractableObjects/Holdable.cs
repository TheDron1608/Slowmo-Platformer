using System;
using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class Holdable : Interactable
{
    const int ON_GRAB_SORTING_ORDER_ADD = 50;
    const float STUCK_IN_WALL_STRINGHT = 40f;
    const float DISABLE_GRAVITY_DURATION_SECONDS = 1f;
    const float MIN_VELOCITY_TO_DISABLE_GRAVITY = 10f;

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
    public int AIPickUpPriority = 0;
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
    private Vector2 _velocitySpeedPreviousFrame = Vector2.zero;
    private bool _isStuck = false;
    private Coroutine _enableGravityCoroutine;
    private CharacterComponentsManager _excludedCollideThrower;

    public event EventHandler<CharacterHoldingObjects> OnGiven;
    public event EventHandler<OnThrownEventArgs> OnThrown;

    public CharacterComponentsManager ExcludedCollideThrower
    {
        get => _excludedCollideThrower;
        private set
        {
            if (_excludedCollideThrower != null)
            {
                foreach (CharacterPart charPart in _excludedCollideThrower.CharacterPartsManager.CharacterParts)
                {
                    if (charPart is CharacterLimbPart limbPart)
                    {
                        Physics2D.IgnoreCollision(limbPart.CharPartHitbox.GetCollider(), _colliderComponent, false);
                        Physics2D.IgnoreCollision(limbPart.CharPartHitbox.GetCollider(), _thrownColliderComponent, false);
                    }
                }
                Physics2D.IgnoreCollision(_excludedCollideThrower.CharacterRigidBodyCapsuleCollider, _colliderComponent, false);
                Physics2D.IgnoreCollision(_excludedCollideThrower.CharacterRigidBodyCapsuleCollider, _thrownColliderComponent, false);
            }

            _excludedCollideThrower = value;

            if (_excludedCollideThrower != null)
            {
                foreach (CharacterPart charPart in _excludedCollideThrower.CharacterPartsManager.CharacterParts)
                {
                    if (charPart is CharacterLimbPart limbPart)
                    {
                        Physics2D.IgnoreCollision(limbPart.CharPartHitbox.GetCollider(), _colliderComponent, true);
                        Physics2D.IgnoreCollision(limbPart.CharPartHitbox.GetCollider(), _thrownColliderComponent, true);
                    }
                }
                Physics2D.IgnoreCollision(_excludedCollideThrower.CharacterRigidBodyCapsuleCollider, _colliderComponent, true);
                Physics2D.IgnoreCollision(_excludedCollideThrower.CharacterRigidBodyCapsuleCollider, _thrownColliderComponent, true);
            }
        }
    }

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
                        VectorMath.Vec2ToDistance(_velocitySpeedPreviousFrame) >= SpeedToGetThrough &&
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

            if (VectorMath.Vec2ToDistance(_velocitySpeedPreviousFrame) <= SpeedToGetThrough)
            {
                _colliderComponent.enabled = true;
                _thrownColliderComponent.enabled = false;
            }
            else
            {
                _colliderComponent.enabled = false;
                _thrownColliderComponent.enabled = true;
            }

            if (VectorMath.Vec2ToDistance(_velocitySpeedPreviousFrame) <= SpeedToHitCharacter)
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
        _velocitySpeedPreviousFrame = _rigidBodyComponent.linearVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isStuck) return;
        if (collision.collider.TryGetComponent(out AbstractCharacterComponent charComponent) && charComponent.CharComponents.CharacterHolding == LastHolder) return;

        if (VectorMath.Vec2ToDistance(_velocitySpeedPreviousFrame) >= SpeedToGetThrough)
        {
            StuckedToCollider = collision.collider;
        }

        if (charComponent != null && VectorMath.Vec2ToDistance(_velocitySpeedPreviousFrame) >= SpeedToHitCharacter)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                collision.contacts[0].point,
                _velocitySpeedPreviousFrame.normalized,
                1f
                );
            for (int i =  0; i < hits.Length; i++)
            {
                if (hits[i].collider.transform.parent.TryGetComponent(out CharacterPart charPartHealth))
                {
                    if (AbstractCharacterComponent.GetCharacterComponentsEqual(charPartHealth, charComponent))
                    {
                        charPartHealth.CharComponents.CharacterEffects.ApplyEffect(EffectsOnThrowHit, this, charPartHealth);
                    }
                }
            }
        }

        _rigidBodyComponent.gravityScale = 1f;
        if (_enableGravityCoroutine != null)
        {
            StopCoroutine(_enableGravityCoroutine);
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

    public void TransformToAnotherObject(Holdable anotherObject)
    {
        Holdable replaceObject = Instantiate(anotherObject, transform.parent);

        replaceObject.TranformSelfToAnotherObject(this);
    }

    public void TranformSelfToAnotherObject(Holdable anotherObject)
    {
        if (gameObject.TryGetComponent(out ThrowableWeapon selfWeapon) && anotherObject.TryGetComponent(out ThrowableWeapon anotherWeapon))
        {
            selfWeapon.IsThrown = anotherWeapon.IsThrown;
        }
        if (gameObject.TryGetComponent(out RangedWeapon selfRangedWeapon) && anotherObject.TryGetComponent(out RangedWeapon anotherRangedWeapon))
        {
            selfRangedWeapon.LoadedLivingAmmoLeft = anotherRangedWeapon.LoadedLivingAmmoLeft;
            selfRangedWeapon.LoadedSpentAmmoLeft = anotherRangedWeapon.LoadedSpentAmmoLeft;
            selfRangedWeapon.AmmoLeft = anotherRangedWeapon.AmmoLeft;
            selfRangedWeapon.Unloaded = anotherRangedWeapon.Unloaded;
        }

        LayerManager.Instance.ChangeZIndexForGameObject(
            LayerManager.Instance.GetZLayerOfGameObject(anotherObject.gameObject),
            gameObject,
            anotherObject.gameObject
            );

        CharacterHoldingObjects newHolder = anotherObject.CurrentHolder;
        Destroy(anotherObject.gameObject);
        if (newHolder != null)
        {
            OnPickedUp(newHolder);
        }
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
        transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesContainer.transform;
        _spriteRendererComponent.sortingOrder -= ON_GRAB_SORTING_ORDER_ADD;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new Vector3(0f, direction.x < 0f ? 180f : 0f, direction.y * 90f);
        transform.rotation = newRotation;

        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
        _rigidBodyComponent.linearVelocity = direction * CurrentHolder.ThrowForce * throwForceMultiplier * ThrowForceMultiplier;
        if (CurrentHolder.TryGetComponent(out CharacterVisual characterVisual))
        {
            _rigidBodyComponent.angularVelocity = ThrowRotationForce * (characterVisual.FlippedH ? -1f : 1f);
        }
        else
        {
            _rigidBodyComponent.angularVelocity = ThrowRotationForce;
        }

        if (VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity) >= MIN_VELOCITY_TO_DISABLE_GRAVITY)
        {
            _rigidBodyComponent.gravityScale = 0f;
            _enableGravityCoroutine = StartCoroutine(EnableGravityAfterDelay());
        }

        CurrentHolder.CurrentHoldObject = null;
        CurrentHolder = null;

        //logic for weapon component and weapon class children classes
        if (TryGetComponent(out Weapon weapon)) 
        {
            for (int i = 0; i < weapon.Projectiles.Count; i++)
            {
                if (weapon.Projectiles[i] is MeleeProjectile)
                {
                    weapon.Projectiles[i].RemoveSelf();
                }
            }
        }
        if (TryGetComponent(out ThrowableWeapon throwableWeapon))
        {
            throwableWeapon.IsThrown = true;
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
    private IEnumerator EnableGravityAfterDelay()
    {
        yield return new WaitForSeconds(DISABLE_GRAVITY_DURATION_SECONDS);
        _rigidBodyComponent.gravityScale = 1f;
    }

    protected virtual void OnPickedUp(CharacterHoldingObjects newHolder)
    {
        newHolder.CurrentHoldObject = this;
        _isStuck = false;

        if (CurrentHolder != newHolder && CurrentHolder != null)
        {
            CurrentHolder.ForceThrow(Vector2.zero);
        }

        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
        CurrentHolder = newHolder;
        //transform.parent = newHolder.transform;
        if (ResetRotationWhenIsHolded)
        {
            Quaternion baseRotation = new();
            baseRotation.eulerAngles = Vector3.zero;
            transform.rotation = baseRotation;
        }
        _spriteRendererComponent.sortingOrder += ON_GRAB_SORTING_ORDER_ADD;
        StuckedToCollider = null;

        ExcludedCollideThrower = newHolder.CharComponents;

        //logic for weapon component and weapon class children classes
        if (TryGetComponent(out ThrowableWeapon throwableWeapon))
        {
            throwableWeapon.IsThrown = false;
        }

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

    private void OnDestroy()
    {
        if (CurrentHolder != null && CurrentHolder.CurrentHoldObject == this)
        {
            CurrentHolder.CurrentHoldObject = null;
        }
    }
}
