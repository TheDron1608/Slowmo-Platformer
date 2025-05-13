using System.Collections;
using System.Linq;
using UnityEngine;

public class GrabAndThrowDangerousWeapons : AbstractAIGrabbingAndThrowing
{
    public float ThrowAfterGrabDelay = 0.5f;

    private bool _isReadyToThrow = false;

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents.CharacterHolding.OnPickedUpHoldable += CharacterHolding_OnPickedUpHoldable;
        CharComponents.CharacterHolding.OnThrewHoldable += CharacterHolding_OnThrewHoldable;
    }

    private void CharacterHolding_OnPickedUpHoldable(object sender, Holdable e)
    {
        StartCoroutine(AwaitDelayThenThrow());
    }

    private void CharacterHolding_OnThrewHoldable(object sender, CharacterHoldingObjects.OnThewEventArgs e)
    {
        _isReadyToThrow = false;
    }

    private void FixedUpdate()
    {
        if (_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable != null && _selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable.CurrentHolder != CharComponents.CharacterHolding)
        {
            CharComponents.CharacterHolding.TryGrab(_selfStateBehaviourAI.PrefferedHoldable.NearestPrefferedHoldable, false);
        }

        if (
            _isReadyToThrow &&
            _selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy != null &&
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.GetIsDangerousAsThrowable(CharComponents.CharacterHolding)
            )
        {
            CharComponents.CharacterHolding.TryThrow((_selfStateBehaviourAI.NearestEnemyInfo.NearestEnemy.CharComponents.Center.transform.position - CharComponents.Center.transform.position).normalized);
        }
    }

    private IEnumerator AwaitDelayThenThrow()
    {
        yield return new WaitForSeconds(ThrowAfterGrabDelay);
        _isReadyToThrow = true;
    }

    private void OnDestroy()
    {
        CharComponents.CharacterHolding.OnPickedUpHoldable -= CharacterHolding_OnPickedUpHoldable;
        CharComponents.CharacterHolding.OnThrewHoldable -= CharacterHolding_OnThrewHoldable;
    }
}
