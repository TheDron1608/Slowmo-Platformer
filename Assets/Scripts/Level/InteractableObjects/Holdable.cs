using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Holdable : Interactable, IStuckableObject
{
    const int ON_GRAB_SORTING_ORDER_ADD = 200;
    const float DISABLE_GRAVITY_DURATION_SECONDS = 1f;
    const float MIN_VELOCITY_TO_DISABLE_GRAVITY = 10f;

    public enum HolsteredFlippingModes
    {
        DONT_FLIP,
        FLIP_CONTANTLY,
        FLIP_ON_CHARACTER_FLIPPED,
        FLIP_ON_CHARACTER_FLIPPED_REVERSED
    }

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
    public string FindingUniqueCodeName;
    public int AIPickUpPriority = 0;
    public bool RotatableWhenIsHolded = true;
    public bool ResetRotationWhenIsHolded = false;
    public float HoldDistanceWhenIsHolded = 0.75f;
    public float ThrowForceMultiplier = 1.0f;
    public float ThrowRotationForce = 12.5f;
    public float SpeedToHitCharacter = 7.5f;
    public float SpeedToGetThrough = 15f;
    public float AimSpeedMultiplier = 1f;
    public List<AbstractEffect> EffectsOnThrowHit = new();
    public List<AbstractEffect> EffectOnHolded = new();
    public bool BreakSelfOnCollide = false;
    public float AngleOnHolstered = 0f;
    public HolsteredFlippingModes FlipXOnHolstered;
    public HolsteredFlippingModes FlipYOnHolstered;
    //public AbstractSoundPlayer SoundOnPickedUp;
    //public AbstractSoundPlayer SoundOnThrown;
    public Sound SoundOnCollide;
    public Sound SoundOnStuck;
    [SerializeField] private bool _hitableWhenIsHolded = false;
    [SerializeField] private bool _hitableWhenIsThrown = false;

    private CharacterHoldingObjects _currentHolder = null;
    private CharacterHoldingObjects _lastHolder = null;
    private CharacterHoldingObjects _telekinesisAffector = null;

    private Rigidbody2D _rigidBodyComponent;
    private BoxCollider2D _colliderComponent;
    private CircleCollider2D _thrownColliderComponent;
    private ObjectEffectsReceiver _effectsReceiver;
    private SoundPlayerOnCollide _collideSoundPlayer;

    private Collider2D _stuckedToCollider = null;
    private Quaternion _rotationPrevFrame = Quaternion.identity;
    private Vector2 _velocitySpeedPreviousFrame = Vector2.zero;
    private bool _isStuck = false;
    private bool _isHolstered = false;
    private Coroutine _enableGravityCoroutine;
    private CharacterComponentsManager _excludedCollideThrower;
    private string _localizedName = "";
    private List<AbstractEffect> _appliedHolderEffects = new();
    private float _extraHoldDistance = 0f;

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
                if (value.TryGetComponent(out IStuckToObject stuckToObject))
                {
                    _stuckedToCollider = value;
                    stuckToObject.AddStuckedObject(this);
                }
                else if (value.TryGetComponent(out AbstractCharacterComponent charComponent))
                {
                    _stuckedToCollider = charComponent.CharComponents.CharacterRigidBodyCapsuleCollider;
                    charComponent.CharComponents.CharacterStuckedObjects.AddStuckedObject(this);
                }
                else
                {
                    _stuckedToCollider = value;
                }

                _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
                _isStuck = true;

                transform.rotation = _rotationPrevFrame;
            }

            else
            {
                _isStuck = false;
                if (_stuckedToCollider.TryGetComponent(out AbstractCharacterComponent charComponent))
                {
                    charComponent.CharComponents.CharacterStuckedObjects.RemoveStuckedObject(this);
                }
                _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;

                _stuckedToCollider = value;
            }

        }
    }

    public bool HitableWhenIsHolded
    {
        get => _hitableWhenIsHolded;
        set
        {
            if (_hitableWhenIsHolded == value || gameObject.IsDestroyed()) return;

            _hitableWhenIsHolded = value;
            gameObject.layer = GetIsHitableNow() ?
                LayerManager.Instance.GetZLayerOfGameObject(gameObject).HitableHoldablesLayer :
                LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;

            if (CurrentHolder != null)
            {
                if (HitableWhenIsHolded)
                {
                    _rigidBodyComponent.simulated = true;
                    _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
                    gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HitableHoldablesLayer;
                }
                else
                {
                    _rigidBodyComponent.simulated = false;
                    _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
                    gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;
                }
            }
            else
            {
                _rigidBodyComponent.simulated = true;
                _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
                gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;
            }
        }
    }

    public bool IsHolstered
    {
        get => _isHolstered;
    }

    public bool GetIsHitableNow()
    {
        return
            (_hitableWhenIsHolded && CurrentHolder != null) ||
            (_hitableWhenIsThrown && CurrentHolder == null);
    }

    public ObjectEffectsReceiver EffectsReceiver
    {
        get => _effectsReceiver;
    }

    public CharacterHoldingObjects CurrentOrLastHolder
    {
        get => _currentHolder ?? _lastHolder;
    }

    public string GetLocalizedName()
    {
        return _localizedName;
    }

    public void SetLocalizedName(string value)
    {
        _localizedName = value;
    }

    public float ExtraHoldDistance
    {
        get => _extraHoldDistance;
        set => _extraHoldDistance = value;
    }

    public bool GetIsThrown()
    {
        return CurrentHolder == null;
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
        if (!TryGetComponent(out _effectsReceiver)) throw new UnityException("EffectsReceiver component not found");
        if (!TryGetComponent(out _collideSoundPlayer)) throw new UnityException("SoundPlayerOnCollide component not found");
        _spriteRendererComponent.sortingOrder += (int)(UnityEngine.Random.value * 99f);
    }

    private void FixedUpdate()
    {
        UpdateStuckStatus();
        _velocitySpeedPreviousFrame = _rigidBodyComponent.linearVelocity;
        _rotationPrevFrame = transform.rotation;
    }

    private void UpdateStuckStatus()
    {
        if (_isStuck)
        {
            _rigidBodyComponent.excludeLayers = int.MaxValue; //excludes all layers
        }
        else if (CurrentHolder == null)
        {
            _rigidBodyComponent.excludeLayers = 0;

            if (VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity) <= SpeedToGetThrough)
            {
                _colliderComponent.enabled = true;
                _thrownColliderComponent.enabled = false;
            }
            else
            {
                _colliderComponent.enabled = false;
                _thrownColliderComponent.enabled = true;
            }

            if (VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity) >= SpeedToHitCharacter)
            {
                _rigidBodyComponent.includeLayers = 1 << LayerManager.Instance.GetZLayerOfGameObject(gameObject)?.CharactersLayer ?? 0;
            }
            else
            {
                _rigidBodyComponent.includeLayers = 0;
            }

            Sound newSound = VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity) > SpeedToGetThrough ? SoundOnStuck : SoundOnCollide;
            if (newSound != null)
            {
                _collideSoundPlayer.SoundPlayer.DefaultSound = newSound;
            }
        }
    }

    private void OnDisable()
    {
        _rigidBodyComponent.includeLayers = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (
            _isStuck ||
            collision.collider.TryGetComponent(out AbstractCharacterComponent charComponent) &&
            charComponent.CharComponents.CharacterHolding == (TelekinesisAffector ?? LastHolder)
            )
        {
            return;
        }

        if (charComponent != null && GetIsDangerouslyFast())
        {
            CharacterLimbPart closestPart = null;
            float closestPartDistance = float.MaxValue;
            foreach (CharacterPart part in charComponent.CharComponents.CharacterPartsManager.CharacterParts)
            {
                if (part.TryGetComponent(out CharacterLimbPart limbpart) && limbpart.CharPartHitbox.TryGetComponent(out Collider2D limbCollider))
                {
                    float distance = Vector2.Distance(limbCollider.bounds.center, limbCollider.bounds.ClosestPoint(_thrownColliderComponent.bounds.center));
                    if (distance < closestPartDistance )
                    {
                        closestPartDistance = distance;
                        closestPart = limbpart;
                    }

                    limbpart.CharPartHealth.ApplyThrowHit(this);
                }
            }

            charComponent.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnThrowHit, this, closestPart);
        }
        else if (GetIsDangerouslyFast() && GameObjectUtility.TryGetComponentInSelfOrParent(collision.gameObject, out ObjectEffectsReceiver effectsReceiver))
        {
            effectsReceiver.ApplyEffect(EffectsOnThrowHit, this);
        }
        
        if (VectorMath.Vec2ToDistance(_velocitySpeedPreviousFrame) >= SpeedToGetThrough)
        {
            StuckedToCollider = collision.collider;
        }

        _rigidBodyComponent.gravityScale = 1f;
        if (_enableGravityCoroutine != null)
        {
            StopCoroutine(_enableGravityCoroutine);
        }

        if (BreakSelfOnCollide && GetIsDangerouslyFast() && TryGetComponent(out BreakableObject breakable))
        {
            breakable.BreakObject(CurrentOrLastHolder);
        }
    }

    public CharacterHoldingObjects CurrentHolder
    {
        get => _currentHolder;
        private set
        {
            if (_currentHolder != null)
            {
                _lastHolder = _currentHolder;
                _telekinesisAffector = null;
            }

            _currentHolder = value;
        }
    }

    public CharacterHoldingObjects LastHolder
    {
        get => _lastHolder;
        private set => _lastHolder = value;
    }

    public CharacterHoldingObjects TelekinesisAffector
    {
        get => _telekinesisAffector;
        set => _telekinesisAffector = value;
    }

    public bool GetIsDangerousAsThrowable(CharacterHoldingObjects thrower)
    {
        return thrower.ThrowForce * ThrowForceMultiplier >= SpeedToHitCharacter;
    }

    public void Give(CharacterHoldingObjects newHolder, bool includeApplyHoldableToCharacter = true)
    {
        if (newHolder == null) throw new UnityException("Holdable.OnPickedUP newHolder argument can not be null, use Throw if you want to unsed holder instead");
        if (CurrentHolder == newHolder) return;

        if (includeApplyHoldableToCharacter)
        {
            if (CurrentHolder != null && CurrentHolder.CurrentHoldObject != null)
            {
                if (!CurrentHolder.TryThrow(Vector2.zero)) return;
            }
            if (newHolder.CurrentHoldObject != null)
            {
                if (!newHolder.TryThrow(new Vector2((newHolder.CharComponents.CharacterVisual.FlippedH ? -1f : 1f), 0.5f), 0.1f)) return;
            }

            newHolder.CurrentHoldObject = this;
            newHolder.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectOnHolded, this);
        }

        Selected = false;
        _isStuck = false;
        _isHolstered = false;

        if (HitableWhenIsHolded)
        {
            _rigidBodyComponent.simulated = true;
            _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
            gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HitableHoldablesLayer;
        }
        else
        {
            _rigidBodyComponent.simulated = false;
            _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
            gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;
        }

        CurrentHolder = newHolder;
        //transform.parent = newHolder.transform;
        if (ResetRotationWhenIsHolded)
        {
            Quaternion baseRotation = new();
            baseRotation.eulerAngles = Vector3.zero;
            transform.rotation = baseRotation;
        }

        StuckedToCollider = null;

        ExcludedCollideThrower = newHolder.CharComponents;

        _appliedHolderEffects = _effectsReceiver.ApplyEffect(newHolder.EffectsOnHoldedObject, newHolder);

        //SoundOnPickedUp.PlaySound();

        _spriteRendererComponent.sortingOrder += ON_GRAB_SORTING_ORDER_ADD;

        if (TryGetComponent(out IThrowableIteractableObj throwableWeapon))
        {
            throwableWeapon.IsThrown = false;
        }

        if (TryGetComponent(out RangedWeapon rangedWeapon) && CurrentHolder.TryGetComponent(out CharacterReloading holderReloading))
        {
            rangedWeapon.SetReloadSpeed(holderReloading.ReloadSpeed);
            rangedWeapon.AttackCooldownMultiplier /= newHolder.CharComponents.CharacterAttacking.AttackCooldownMultiplier;
        }

        if (TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
        {
            if (magReloadingWeapon.Unloaded && magReloadingWeapon.Mags > 0)
            {
                magReloadingWeapon.TryCloseMag();
            }
            else if (!magReloadingWeapon.BulletLoadedInChamber)
            {
                magReloadingWeapon.ReloadBullet();
            }
        }

        if (TryGetComponent(out BulletReloadingWeapon bulletReloadWeapon))
        {
            if (bulletReloadWeapon.Unloaded && bulletReloadWeapon.LoadedLivingAmmoLeft > 0)
            {
                bulletReloadWeapon.TryCloseMag();
            }
        }

        if (TryGetComponent(out BoltReloadingWeapon boltReloadingWeapon))
        {
            if (!boltReloadingWeapon.BulletLoadedInChamber)
            {
                boltReloadingWeapon.UnloadBullet();
            }
        }

        if (TryGetComponent(out SpinableMeleeWeapon spinableMeleeWeapon))
        {
            spinableMeleeWeapon.Spin();
        }

        if (TryGetComponent(out HolsterableMeleeWeapon holsterableMeleeWeapon))
        {
            if (LastHolder != CurrentHolder)
            {
                holsterableMeleeWeapon.IsHolstered = !holsterableMeleeWeapon.IsHolstered;
            }
        }

        OnGiven?.Invoke(this, newHolder);
    }

    public void Throw(Vector2 direction, float throwForceMultiplier = 1f)
    {
        OnThrown?.Invoke(this, new OnThrownEventArgs(CurrentHolder, direction));

        if (CurrentHolder == null) return;

        _isStuck = false;
        _isHolstered = false;
        transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesContainer.transform;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new Vector3(0f, direction.x < 0f ? 180f : 0f, direction.y * 90f);
        transform.rotation = newRotation;

        _rigidBodyComponent.simulated = true;
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

        if (HitableWhenIsHolded)
        {
            gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;
        }

        UpdateStuckStatus();

        if (VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity) >= MIN_VELOCITY_TO_DISABLE_GRAVITY)
        {
            _rigidBodyComponent.gravityScale = 0f;
            _enableGravityCoroutine = StartCoroutine(EnableGravityAfterDelay());
        }

        _effectsReceiver.RemoveEffect(_appliedHolderEffects);
        CurrentHolder.CharComponents.CharacterEffectsReceiver.RemoveEffect(EffectOnHolded);
        _appliedHolderEffects = new();

        //SoundOnThrown.PlaySound();
        _spriteRendererComponent.sortingOrder -= ON_GRAB_SORTING_ORDER_ADD;

        if (TryGetComponent(out Weapon weapon))
        {
            for (int i = 0; i < weapon.Projectiles.Count; i++)
            {
                if (weapon.Projectiles[i] is MeleeProjectile)
                {
                    weapon.Projectiles[i].RemoveProjectile();
                }
            }
        }

        if (TryGetComponent(out IThrowableIteractableObj throwableWeapon))
        {
            throwableWeapon.IsThrown = true;
        }
        if (TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SetReloadSpeed(1f);
            rangedWeapon.AttackCooldownMultiplier *= CurrentHolder.CharComponents.CharacterAttacking.AttackCooldownMultiplier;
        }
        if (TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon))
        {
            if (hammerWeapon.IsHammerring)
            {
                hammerWeapon.TrySetHammered(false);
                hammerWeapon.SoundOnHammer.BreakAllSounds();
            }
        }
        if (TryGetComponent(out Chainsaw chainsaw))
        {
            chainsaw.Started = false;
        }
        if (TryGetComponent(out HolsterableMeleeWeapon holsterableMeleeWeapon))
        {
            if (IsHolstered)
            {
                holsterableMeleeWeapon.IsHolstered = true;
            }
        }

        CurrentHolder.CurrentHoldObject = null;
        CurrentHolder = null;
    }

    public void Holster(CharacterHoldingObjects holsterer)
    {
        if (holsterer == null) throw new UnityException("Holdable.OnPickedUP newHolder argument can not be null, use Unholster if you want to unsed holder instead");
        CurrentHolder = holsterer;

        if (CurrentHolder.CurrentHolsteredHoldObject == this) return;

        if (CurrentHolder.CurrentHolsteredHoldObject == this)
        {
            if (!CurrentHolder.TryUnholster()) return;
        }
        else if (CurrentHolder.CurrentHoldObject == this)
        {
            if (!CurrentHolder.TryThrow(Vector2.zero)) return;
        }

        if (holsterer.CurrentHolsteredHoldObject != null)
        {
            if (!holsterer.TryUnholster()) return;
        }

        Selected = false;
        holsterer.CurrentHolsteredHoldObject = this;
        _isStuck = false;
        _isHolstered = true;

        if (HitableWhenIsHolded)
        {
            _rigidBodyComponent.simulated = true;
            _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
            gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HitableHoldablesLayer;
        }
        else
        {
            _rigidBodyComponent.simulated = false;
            _rigidBodyComponent.bodyType = RigidbodyType2D.Static;
            gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;
        }

        CurrentHolder = holsterer;
        //transform.parent = newHolder.transform;
        if (ResetRotationWhenIsHolded)
        {
            Quaternion baseRotation = new();
            baseRotation.eulerAngles = Vector3.zero;
            transform.rotation = baseRotation;
        }

        StuckedToCollider = null;

        ExcludedCollideThrower = CurrentHolder.CharComponents;

        _appliedHolderEffects = _effectsReceiver.ApplyEffect(holsterer.EffectsOnHoldedObject, holsterer);
        CurrentHolder.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectOnHolded, this);

        //SoundOnPickedUp.PlaySound();

        _spriteRendererComponent.sortingOrder += ON_GRAB_SORTING_ORDER_ADD;

        if (TryGetComponent(out HolsterableMeleeWeapon holsterableMeleeWeapon))
        {
            holsterableMeleeWeapon.IsHolstered = true;
        }

        if (TryGetComponent(out Shield shield))
        {
            shield.TryRaiseUp();
        }

        if (TryGetComponent(out IThrowableIteractableObj throwableWeapon))
        {
            throwableWeapon.IsThrown = true;
        }
    }

    public void Unholster()
    {
        if (CurrentHolder == null) return;

        _isStuck = false;
        _isHolstered = false;
        transform.parent = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesContainer.transform;

        _rigidBodyComponent.simulated = true;
        _rigidBodyComponent.bodyType = RigidbodyType2D.Dynamic;
        _rigidBodyComponent.linearVelocity = Vector2.zero;

        if (HitableWhenIsHolded)
        {
            gameObject.layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject).HoldablesLayer;
        }

        UpdateStuckStatus();

        if (VectorMath.Vec2ToDistance(_rigidBodyComponent.linearVelocity) >= MIN_VELOCITY_TO_DISABLE_GRAVITY)
        {
            _rigidBodyComponent.gravityScale = 0f;
            _enableGravityCoroutine = StartCoroutine(EnableGravityAfterDelay());
        }

        _effectsReceiver.RemoveEffect(_appliedHolderEffects);
        CurrentHolder.CharComponents.CharacterEffectsReceiver.RemoveEffect(EffectOnHolded);
        _appliedHolderEffects = new();

        //SoundOnThrown.PlaySound();
        _spriteRendererComponent.sortingOrder -= ON_GRAB_SORTING_ORDER_ADD;

        if (TryGetComponent(out Weapon weapon))
        {
            for (int i = 0; i < weapon.Projectiles.Count; i++)
            {
                if (weapon.Projectiles[i] is MeleeProjectile)
                {
                    weapon.Projectiles[i].RemoveProjectile();
                }
            }
        }

        if (TryGetComponent(out IThrowableIteractableObj throwableWeapon))
        {
            throwableWeapon.IsThrown = true;
        }
        if (TryGetComponent(out RangedWeapon rangedWeapon))
        {
            rangedWeapon.SetReloadSpeed(1f);
            rangedWeapon.AttackCooldownMultiplier *= CurrentHolder.CharComponents.CharacterAttacking.AttackCooldownMultiplier;
        }
        if (TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon))
        {
            if (hammerWeapon.IsHammerring)
            {
                hammerWeapon.TrySetHammered(false);
                hammerWeapon.SoundOnHammer.BreakAllSounds();
            }
        }
        if (TryGetComponent(out Chainsaw chainsaw))
        {
            chainsaw.Started = false;
        }
        if (TryGetComponent(out HolsterableMeleeWeapon holsterableMeleeWeapon))
        {
            if (IsHolstered)
            {
                holsterableMeleeWeapon.IsHolstered = true;
            }
        }

        CurrentHolder.CurrentHolsteredHoldObject = null;
        CurrentHolder = null;
    }

    public bool GetIsDangerouslyFast()
    {
        return !_isStuck && _velocitySpeedPreviousFrame.magnitude >= SpeedToHitCharacter && _rigidBodyComponent.simulated;
    }

    public void TransformToAnotherObject(Holdable anotherObject)
    {
        Holdable newHoldable = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TrySpawnObject(
            anotherObject.gameObject,
            transform.position,
            null,
            null
            )?.FirstOrDefault()?.GetComponent<Holdable>();

        newHoldable?.TranformSelfToAnotherObject(this);
    }

    public void TranformSelfToAnotherObject(Holdable anotherObject)
    {
        if (gameObject.TryGetComponent(out IThrowableIteractableObj selfWeapon) && anotherObject.TryGetComponent(out IThrowableIteractableObj anotherWeapon))
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
        if (newHolder != null && newHolder.CurrentHoldObject == anotherObject)
        {
            Give(newHolder);
        }
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return
            base.StartInteractCondition(interactor) &&
            (
                CurrentHolder == null ||
                (
                    interactor.TryGetComponent(out CharacterHoldingObjects holder) &&
                    holder.CanDisarm &&
                    CurrentHolder != holder
                )
            );
    }

    protected override void OnStartInteact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out CharacterHoldingObjects charHoldingObjects))
        {
            charHoldingObjects.TryGrab(this);
        }
    }

    private IEnumerator EnableGravityAfterDelay()
    {
        yield return new WaitForSeconds(DISABLE_GRAVITY_DURATION_SECONDS);
        _rigidBodyComponent.gravityScale = 1f;
    }

    public override void InvokeOnEffectApllied(AbstractEffect Effect, ObjectEffectsReceiver Receiver)
    {
        base.InvokeOnEffectApllied(Effect, Receiver);
        CurrentOrLastHolder?.CharComponents.CharacterAttacking?.InvokeOnEffectApllied(Effect, Receiver);
    }

    private void OnEnable()
    {
        if (CurrentHolder != null)
        {
            if (TryGetComponent(out MagReloadingWeapon magReloadingWeapon))
            {
                if (magReloadingWeapon.Unloaded && magReloadingWeapon.Mags > 0)
                {
                    magReloadingWeapon.TryCloseMag();
                }
                else if (!magReloadingWeapon.BulletLoadedInChamber)
                {
                    magReloadingWeapon.ReloadBullet();
                }
            }

            if (TryGetComponent(out BulletReloadingWeapon bulletReloadWeapon))
            {
                if (bulletReloadWeapon.Unloaded && bulletReloadWeapon.LoadedLivingAmmoLeft > 0)
                {
                    bulletReloadWeapon.TryCloseMag();
                }
            }

            if (TryGetComponent(out BoltReloadingWeapon boltReloadingWeapon))
            {
                if (!boltReloadingWeapon.BulletLoadedInChamber)
                {
                    boltReloadingWeapon.UnloadBullet();
                }
            }
        }
    }

    protected override string GetSelectInfoText()
    {
        string result = "<size=80%>" + GetLocalizedName() + "</size>";

        if (TryGetComponent(out ThrowableWeapon weapon))
        {
            result += "\n" + weapon.GetAmmoInfoOnSelect();
        }

        return result;
    }

    protected override bool SelectInfoAppearCondition()
    {
        return base.SelectInfoAppearCondition() && !GetIsDangerouslyFast();
    }

    private void OnDestroy()
    {
        if (CurrentHolder != null && CurrentHolder.CurrentHoldObject == this)
        {
            CurrentHolder.CurrentHoldObject = null;
        }
    }
}
