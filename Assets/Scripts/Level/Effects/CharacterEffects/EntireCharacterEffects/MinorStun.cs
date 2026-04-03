using UnityEngine;

public class MinorStun : AbstractStun
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();
        AffectedCharacter.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.MINOR_STUN;
        AffectedCharacter.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;

        AffectedCharacter.CharacterMoving.IsAbleToMove = false;
        AffectedCharacter.CharacterJumping.IsAbleToJump = false;
        AffectedCharacter.CharacterInteract.IsAbleToInteractWithObjects = false;
        AffectedCharacter.CharacterHolding.IsAbleToGrabObjects = false;
        AffectedCharacter.CharacterHolding.IsAbleToThrowObjects = false;
        AffectedCharacter.CharacterAiming.IsAbleToAim = false;
        AffectedCharacter.CharacterAttacking.IsAbleToAttack = false;
        AffectedCharacter.CharacterAttacking.IsAbleToHammer = false;
        AffectedCharacter.CharacterAttacking.IsAbleToStartChainsaw = false;
        AffectedCharacter.CharacterReloading.IsAbleToReload = false;
        AffectedCharacter.CharacterRolling.IsAbleToRoll = false;
        AffectedCharacter.CharacterInteractionWithTiles.IsCurrentAbleToStickOnWalls = false;
        if (AffectedCharacter.CharacterSpecial != null)
        {
            AffectedCharacter.CharacterSpecial.IsAbleToDoSpecial = false;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (AffectedCharacter.CharacterVisual.CurrentBusyAnimation == CharacterVisual.CharacterPartBusyStates.MINOR_STUN)
        {
            AffectedCharacter.CharacterVisual.BreakBusyAnimation();
        }
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.OldState == CharacterVisual.CharacterPartBusyStates.MINOR_STUN)
        {
            AffectedCharacter.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
            RemoveSelf();
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            !affectWho.GetHasEffect<HardStun>() &&
            !affectWho.GetHasEffect<Death>();
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        
    }
}
