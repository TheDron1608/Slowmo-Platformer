using Unity.VisualScripting;
using UnityEngine;

public class AIPrefferOnlyFirstGrabbedWeapon : AbstractAIPrefferedHoldable
{
    private Holdable _onlyHoldable = null;
    bool _isFirstFrame = true;

    protected override void OnAwake()
    {
        base.OnAwake();

        CharComponents.CharacterHolding.OnPickedUpHoldable += CharacterHolding_OnPickedUpHoldable;
    }

    private void CharacterHolding_OnPickedUpHoldable(object sender, Holdable e)
    {
        _onlyHoldable = e;
    }

    protected override bool OrderByPattern(Holdable oldHoldable, Holdable newHoldable)
    {
        return
            Vector2.Distance(CharComponents.Center.transform.position, newHoldable.transform.position) <
            Vector2.Distance(CharComponents.Center.transform.position, oldHoldable.transform.position);
    }

    protected override bool PickUpCondition(Holdable holdable)
    {
        return 
            base.PickUpCondition(holdable) &&
            (CharComponents.CharacterHolding.CurrentHoldObject != _onlyHoldable || _onlyHoldable == null) &&
            !_isFirstFrame &&
            (
                _onlyHoldable == null || 
                _onlyHoldable.IsDestroyed() ||
                (_onlyHoldable.TryGetComponent(out RangedWeapon rp) && rp.GetIsOutOfAmmo()) ||
                _onlyHoldable == holdable
            );
    }

    private void LateUpdate()
    {
        _isFirstFrame = false;
    }

    private void OnDestroy()
    {
        CharComponents.CharacterHolding.OnPickedUpHoldable -= CharacterHolding_OnPickedUpHoldable;
    }
}
