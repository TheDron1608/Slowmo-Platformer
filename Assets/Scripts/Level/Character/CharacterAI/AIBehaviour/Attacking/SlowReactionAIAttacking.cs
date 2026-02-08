using System.Collections;
using UnityEngine;

public class SlowReactionAIAttacking : AbstractDelayedAttacking
{
    public float AutoWeaponAttackDurationSeconds = 1f;

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
        CharComponents.CharacterAiming.AimWeaponDown = true;
        if (_attackDelayingCoroutine != null)
        {
            StopCoroutine(_attackDelayingCoroutine);
            _attackDelayingCoroutine = null;
        }
    }
    private IEnumerator AwaitStartAttackDelay(Vector2 targetAim)
    {
        while (CharComponents.CharacterReloading.GetIsReloading())
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<MeleeWeapon>() != null ? MeleeAttackDelaySeconds : RangedAttackDelaySeconds);
        
        if (!CharComponents.CharacterReloading.GetIsReloading())
        {
            if ((CharComponents.CharacterHolding.CurrentHoldObject?.GetComponent<Weapon>() ?? CharComponents.UnarmedAttacking)?.AutoAttack ?? false)
            {
                yield return AttackAutoWeaponSomeTime(targetAim);
            }
            else
            {
                CharComponents.CharacterAttacking.TryAttack(targetAim);
            }
        }
        _attackDelayingCoroutine = null;
    }

    private IEnumerator AttackAutoWeaponSomeTime(Vector2 targetAim)
    {
        for (float t = 0f; t < AutoWeaponAttackDurationSeconds; t += Time.deltaTime)
        {
            CharComponents.CharacterAttacking.TryAttack(targetAim);

            yield return new WaitForEndOfFrame();
        }
    }
}
