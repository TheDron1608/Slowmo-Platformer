using UnityEngine;

public class MinorStun : AbstractOverwritingCharacterEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();
        AffectedCharacter.CharacterVisual.CurrentBusyAnimation = CharacterPart.CharacterPartBusyStates.MINOR_STUN;
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
        AffectedCharacter.CharacterInteractionWithTiles.IsAbleToStickOnWalls = false;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (AffectedCharacter.CharacterEffects.GetHasEffect<HardStun>() || AffectedCharacter.CharacterEffects.GetHasEffect<MinorStun>()) return;

        AffectedCharacter.CharacterMoving.IsAbleToMove = true;
        AffectedCharacter.CharacterJumping.IsAbleToJump = true;
        AffectedCharacter.CharacterInteract.IsAbleToInteractWithObjects = true;
        AffectedCharacter.CharacterHolding.IsAbleToGrabObjects = true;
        AffectedCharacter.CharacterHolding.IsAbleToThrowObjects = true;
        AffectedCharacter.CharacterAiming.IsAbleToAim = true;
        AffectedCharacter.CharacterAttacking.IsAbleToAttack = true;
        AffectedCharacter.CharacterAttacking.IsAbleToHammer = true;
        AffectedCharacter.CharacterAttacking.IsAbleToStartChainsaw = true;
        AffectedCharacter.CharacterReloading.IsAbleToReload = true;
        AffectedCharacter.CharacterRolling.IsAbleToRoll = true;
        AffectedCharacter.CharacterInteractionWithTiles.IsAbleToStickOnWalls = true;
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.OldState == CharacterPart.CharacterPartBusyStates.MINOR_STUN)
        {
            AffectedCharacter.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
            RemoveSelf();
        }
    }

    public override bool ApplyCondition(CharacterComponentsManager affectWho)
    {
        return base.ApplyCondition(affectWho) && !affectWho.CharacterEffects.GetHasEffect<HardStun>() && !affectWho.CharacterHealth.Dead;
    }
}
