using System.Collections;
using UnityEngine;

public class SlowReactionAIAttacking : AbstractDelayedAttacking
{
    private Coroutine _attackDelayingCoroutine = null;

    protected override void OnTrackedEnemy()
    {
        CharComponents.CharacterAiming.AimWeaponDown = false;
        if (_attackDelayingCoroutine == null)
        {
            CharComponents.CharacterAiming.TargetAimPoint = _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position;
            _attackDelayingCoroutine = StartCoroutine(AwaitStartAttackDelay(CharComponents.CharacterAiming.TargetAimPoint));
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
    private IEnumerator AwaitStartAttackDelay(Vector2 targetAim)
    {
        yield return new WaitForSeconds(CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<MeleeWeapon>() != null ? MeleeAttackDelaySeconds : RangedAttackDelaySeconds);
        CharComponents.CharacterAttacking.TryAttack(targetAim);
        _attackDelayingCoroutine = null;
    }
}
