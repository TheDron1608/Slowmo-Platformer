
using UnityEngine;

public class SetCanForceStopRollingOnJump : AbstractCharacterEffect, IEntireCharacterEffect
{
    public bool Value;

    private bool _oldCanForceStopRollingOnJump;

    protected override void OnApply()
    {
        base.OnApply();

        _oldCanForceStopRollingOnJump = AffectedCharacter.CharacterJumping.CanForceStopRollingOnJump;

        AffectedCharacter.CharacterJumping.CanForceStopRollingOnJump = Value;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterJumping.CanForceStopRollingOnJump = _oldCanForceStopRollingOnJump;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<SetCanForceStopRollingOnJump>();
    }
}