using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttacking : AbstractCharacterComponent, IEffectApplier
{
    const float CLUMSY_RANGED_POST_ATTACK_DELAY_SECONDS = 0.25f;

    [SerializeField] private bool _isAbleToAttack = true;
    [SerializeField] private bool _isAbleToHammer = true;
    [SerializeField] private bool _isAbleToStartChainsaw = true;
    [SerializeField] private bool _isAbleToShield = true;
    [SerializeField] private bool _isAbleToArmGrenade = true;
    public float AttackCooldownMultiplier = 1f;
    public List<AbstractEffect> ExtraProjectileEffects = new();

    private Vector2? _awaitingMeleeAttackDirection = null;
    private Coroutine _clumsyRangedAttackCoroutine = null;

    /// <summary>
    /// bool parameter returns successfull attack attempt or not
    /// </summary>
    public event EventHandler<bool> OnAttack;
    public event EventHandler<IEffectApplier.OnEffectAppliedEventArgs> OnEffectApplied;

    public bool IsAbleToAttack
    {
        get => _isAbleToAttack;
        set => _isAbleToAttack = value;
    }
    public bool IsAbleToHammer
    {
        get => _isAbleToHammer;
        set
        {
            if (!value)
            {
                TryStopHammerringWeapon();
            }
            _isAbleToHammer = value;
        }
    }
    public bool IsAbleToStartChainsaw
    {
        get => _isAbleToStartChainsaw;
        set => _isAbleToStartChainsaw = value;
    }
    public bool IsAbleToShield
    {
        get => _isAbleToShield;
        set
        {
            if (!value)
            {
                TryStopShield();
            }
            _isAbleToShield = value;
        }
    }
    public bool IsAbleToArmGrenade
    {
        get => _isAbleToArmGrenade;
        set => _isAbleToArmGrenade = value;
    }

    private void OnEnable()
    {
        _awaitingMeleeAttackDirection = null;
        _clumsyRangedAttackCoroutine = null;
        CharComponents.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
    }

    public bool TryShield()
    {
        if (IsAbleToShield && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Shield shield))
        {
            if (shield.TryRaiseUp())
            {
                CharComponents.CharacterAiming.AimWeaponDown = false;
                if (CharComponents.CharacterClumsyness.ClumsyShielding)
                {
                    CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.CLUMSY_SHIELD;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public bool TryStopShield()
    {
        if (IsAbleToShield && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Shield shield))
        {
            if (shield.TryRaiseDown())
            {
                if (CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.CLUMSY_SHIELD)
                {
                    CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.NONE;
                }
                if (CharComponents.CharacterClumsyness.ClumsyShielding)
                {
                    CharComponents.CharacterAiming.AimWeaponDown = true;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public bool TryHammerWeapon()
    {
        if (IsAbleToHammer && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            if (hammerWeapon.TrySetHammered(true))
            {
                CharComponents.CharacterAiming.AimWeaponDown = false;
                return true;
            }
            return false;
        }
        return false;
    }

    public bool TryStopHammerringWeapon()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out HammerBulletReloadingWeapon hammerWeapon) && !hammerWeapon.Hammered)
        {
            if (hammerWeapon.TrySetHammered(false))
            {
                if (CharComponents.CharacterClumsyness.ClumsyRangedAttack)
                {
                    CharComponents.CharacterAiming.AimWeaponDown = true;
                }
                return true;
            }
            return false;
        }
        return false;
    }

    public bool TryStartChainsaw()
    {
        if (IsAbleToStartChainsaw && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Chainsaw chainsaw) && !chainsaw.Started)
        {
            return chainsaw.TryStart();
        }
        return false;
    }

    public bool TryAttack(Vector2 direction)
    {
        if (
            (CharComponents.CharacterVisual.IsBusy() && !CharComponents.CharacterVisual.IsClumsyAnimation()) ||
            (
                CharComponents.CharacterClumsyness.ClumsyRangedAttack && 
                (CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out RangedWeapon rw) ?? false) && 
                CharComponents.CharacterMoving.GetCurrentMoveDirection() != 0f
            )
            )
        {
            return false;
        }

        if (CharComponents.CharacterClumsyness.GetIsClumsyAttackWithCurrentWeapon())
        {
            if (
                CharComponents.CharacterCollision.IsCollidingFloor() &&
                IsAbleToAttack &&
                CharComponents.CharacterHolding.CurrentHoldObject != null &&
                CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon)
                )
            {
                if (CharComponents.CharacterAiming.GetHoldingValidForAimRangedWeapon())
                {
                    if (CharComponents.CharacterAiming.GetCurrentAimReachedTargetAim() && CharComponents.CharacterAiming.AimPerformed)
                    {
                        return ForceAttack(direction);
                    }
                    else if (_clumsyRangedAttackCoroutine == null)
                    {
                        _clumsyRangedAttackCoroutine = StartCoroutine(AwaitClumsyRangedAttackDelayThenAttack(direction));
                    }
                }
                else
                {
                    if (CharComponents.CharacterVisual.CurrentBusyAnimation != CharacterVisual.CharacterPartBusyStates.CLUMSY_MELEE_ATTACK)
                    {
                        CharComponents.CharacterAiming.AimWeaponDown = false;
                        _awaitingMeleeAttackDirection = direction;

                        CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.CLUMSY_MELEE_ATTACK;
                        CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return ForceAttack(direction);
        }
    }

    public bool TryArmGrenade()
    {
        if (IsAbleToArmGrenade && CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out OnInteractArmGrenade grenade))
        {
            return grenade.TryInteract(gameObject);
        }
        else
        {
            return false;
        }
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.OldState == CharacterVisual.CharacterPartBusyStates.CLUMSY_MELEE_ATTACK && e.NewState == CharacterVisual.CharacterPartBusyStates.NONE && _awaitingMeleeAttackDirection.HasValue)
        {
            ForceAttack(_awaitingMeleeAttackDirection.Value);
            _awaitingMeleeAttackDirection = null;
        }
        CharComponents.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
    }

    private IEnumerator AwaitClumsyRangedAttackDelayThenAttack(Vector2 direction)
    {
        CharComponents.CharacterAiming.AimWeaponDown = false;

        while (!(CharComponents.CharacterAiming.GetCurrentAimReachedTargetAim() && CharComponents.CharacterAiming.AimPerformed))
        {
            if (CharComponents.CharacterAiming.AimWeaponDown) BreakClumsyRangedAttack();
            yield return new WaitForFixedUpdate();
        }

        ForceAttack(direction);

        while (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) &&
            weapon.IsInCooldown
            )
        {
            if (CharComponents.CharacterAiming.AimWeaponDown) BreakClumsyRangedAttack();
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(CLUMSY_RANGED_POST_ATTACK_DELAY_SECONDS);

        _clumsyRangedAttackCoroutine = null;
    }

    public void BreakClumsyRangedAttack()
    {
        if (_clumsyRangedAttackCoroutine != null)
        {
            StopCoroutine(_clumsyRangedAttackCoroutine);
            _clumsyRangedAttackCoroutine = null;
        }

        if (CharComponents.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.AIM)
        {
            CharComponents.CharacterAiming.AimWeaponDown = true;
            CharComponents.CharacterVisual.ForceResetBusyAnimation();
        }
    }

    public bool ForceAttack(Vector2 direction)
    {
        if (!IsAbleToAttack || CharComponents.CharacterAiming.AimWeaponDown) return false;

        if (CharComponents.CharacterHolding.CurrentHoldObject != null)
        {
            if (CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon))
            {
                bool isSuccessfullAttack = weapon.TryAttack(direction);

                if (isSuccessfullAttack)
                {
                    if (TryGetComponent(out CharacterRolling charRolling))
                    {
                        charRolling.ForceStopRolling();
                    }
                }

                OnAttack?.Invoke(this, isSuccessfullAttack);
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (CharComponents.UnarmedAttacking.Projectile != null)
        {
            bool isSuccessfullAttack = CharComponents.UnarmedAttacking.TryAttack(direction);
            OnAttack?.Invoke(this, isSuccessfullAttack);
            return isSuccessfullAttack;
        }
        else
        {
            return false;
        }
    }

    public bool TryUseAttack(Vector2 direction)
    {
        if (TryShield()) return true;

        //if (TryHammerWeapon()) return true;

        if (TryStartChainsaw()) return true;

        if (TryAttack(direction)) return true;

        if (TryArmGrenade()) return true;

        return false;
    }

    public bool TryStopAttack()
    {
        if (TryStopShield()) return true;

        if (TryStopHammerringWeapon()) return true;

        return false;
    }

    public void InvokeOnEffectApllied(AbstractEffect Effect, ObjectEffectsReceiver Receiver)
    {
        OnEffectApplied?.Invoke(this, new(this, Effect, Receiver));
    }
}
