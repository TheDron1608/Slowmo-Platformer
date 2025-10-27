using System.Collections;
using UnityEngine;

public class DefaultAIAttacking : AbstractDelayedAttacking
{
    private Coroutine _attackDelayingCoroutine = null;

    protected override void OnTrackedEnemy()
    {
        CharComponents.CharacterAiming.AimWeaponDown = false;
        CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;
        if (_attackDelayingCoroutine == null)
        {
            _attackDelayingCoroutine = StartCoroutine(AwaitStartAttackDelay());
        }
    }

    protected override void OnLostEnemy()
    {
        if (_attackDelayingCoroutine != null)
        {
            StopCoroutine(_attackDelayingCoroutine);
            _attackDelayingCoroutine = null;
        }
    }

    private IEnumerator AwaitStartAttackDelay()
    {
        yield return new WaitForSeconds(CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<MeleeWeapon>() != null ? MeleeAttackDelaySeconds : RangedAttackDelaySeconds);
        CharComponents.CharacterAttacking.TryAttack(CharComponents.CharacterAiming.GetCurrentAimNormalized());
        _attackDelayingCoroutine = null;
    }
}
