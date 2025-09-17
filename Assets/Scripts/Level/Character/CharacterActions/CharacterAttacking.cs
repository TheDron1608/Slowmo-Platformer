using System;
using System.Collections;
using UnityEngine;

public class CharacterAttacking : AbstractCharacterComponent
{
    const float CLUMSY_RANGED_POST_ATTACK_DELAY_SECONDS = 0.25f;

    [SerializeField] private bool _isAbleToAttack = true;
    [SerializeField] private bool _isAbleToHammer = true;
    [SerializeField] private bool _isAbleToStartChainsaw = true;
    public float AttackCooldownMultiplier = 1f;
    public AbstractProjectile UnarmedAttackProjectile;

    private Vector2? _awaitingMeleeAttackDirection = null;
    private Coroutine _clumsyRangedAttackCoroutine = null;

    /// <summary>
    /// bool parameter returns successfull attack attempt or not
    /// </summary>
    public event EventHandler<bool> OnAttack;

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
            CharComponents.CharacterVisual.IsBusy() &&
            CharComponents.CharacterVisual.CurrentBusyAnimation != CharacterVisual.CharacterPartBusyStates.AIM &&
            CharComponents.CharacterVisual.CurrentBusyAnimation != CharacterVisual.CharacterPartBusyStates.CLUMSY_MELEE_ATTACK
            )
        {
            return false;
        }

        if (CharComponents.CharacterClumsyness.GetIsClumsyAttackWithCurrentWeapon())
        {
            if (
                !CharComponents.CharacterCollision.IsCollidingFloor() ||
                !IsAbleToAttack || 
                CharComponents.CharacterHolding.CurrentHoldObject == null || 
                !CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon)
                )
            {
                return false;
            }

            if (
                CharComponents.CharacterAiming.GetHoldingValidForAimWeapon()
                )
            {
                if (_clumsyRangedAttackCoroutine != null)
                {
                    StopCoroutine(_clumsyRangedAttackCoroutine);
                }
                _clumsyRangedAttackCoroutine = StartCoroutine(AwaitClumsyRangedAttackDelayThenAttack(direction));
            }
            else
            {
                CharComponents.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.CLUMSY_MELEE_ATTACK;
                _awaitingMeleeAttackDirection = direction;
                CharComponents.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;
            }

            return true;
        }
        else
        {
            return ForceAttack(direction);
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
            yield return new WaitForFixedUpdate();
        }

        ForceAttack(direction);

        while (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out Weapon weapon) &&
            weapon.IsInCooldown
            )
        {
            yield return new WaitForFixedUpdate();
        }



        yield return new WaitForSeconds(CLUMSY_RANGED_POST_ATTACK_DELAY_SECONDS);
    }

    public bool ForceAttack(Vector2 direction)
    {
        if (!IsAbleToAttack) return false;

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

    public bool TryLoadElseAttack(Vector2 direction)
    {
        if (TryHammerWeapon()) return true;

        if (TryStartChainsaw()) return true;

        if (TryAttack(direction)) return true;

        return false;
    }
}
