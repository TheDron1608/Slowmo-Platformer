using System.Collections;
using UnityEngine;

public class DefaultAIAttacking : AbstractDelayedAttacking
{
    private Coroutine _attackDelayingCoroutine = null;
    private bool _isAutoWeaponAttacking = false;

    protected override void OnTrackedEnemy()
    {
        CharComponents.CharacterAiming.AimWeaponDown = false;
        CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;
        if (_attackDelayingCoroutine == null)
        {
            if (_isAutoWeaponAttacking)
            {
                if (!CharComponents.CharacterReloading.GetIsReloading())
                {
                    CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
                }
            }
            else
            {
                _attackDelayingCoroutine = StartCoroutine(AwaitStartAttackDelay());
            }
        }
    }

    protected override void OnLostEnemy()
    {
        CharComponents.CharacterAiming.AimWeaponDown = true;
        if (_attackDelayingCoroutine != null)
        {
            StopCoroutine(_attackDelayingCoroutine);
            _attackDelayingCoroutine = null;
        }
        _isAutoWeaponAttacking = false;
    }

    private IEnumerator AwaitStartAttackDelay()
    {
        while (CharComponents.CharacterReloading.GetIsReloading())
        {
            yield return new WaitForFixedUpdate();
        }

        if ((CharComponents.CharacterHolding.CurrentHoldObject?.TryGetComponent(out Weapon weapon) ?? false))
        {
            if (weapon.AutoAttack) _isAutoWeaponAttacking = true;
            yield return new WaitForSeconds(
                CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<MeleeWeapon>() != null ?
                CharComponents.CharacterClumsyness.GetIsClumsyAttackWithCurrentWeapon() && MeleeAttackDelaySeconds < CLUMSY_MELEE_ATTACK_MIN_DELAY ? 0f : MeleeAttackDelaySeconds :
                RangedAttackDelaySeconds
                );
        }
        
        if (!CharComponents.CharacterReloading.GetIsReloading())
        {
            CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
        }
        _attackDelayingCoroutine = null;
    }
}
