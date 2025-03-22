using UnityEngine;

public class HardStun : AbstractStun
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterEffects.RemoveEffect<MinorStun>();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();
        AffectedCharacter.CharacterVisual.CurrentBusyAnimation = CharacterPartVisual.CharacterPartBusyStates.FALLING_IN_AIR;
        AffectedCharacter.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;

        AffectedCharacter.CharacterHolding.TryThrow(AffectedCharacter.CharacterRigidBody.linearVelocity.normalized, 0.25f);

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

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.NewState != CharacterPartVisual.CharacterPartBusyStates.FALLING_IN_AIR && e.OldState == CharacterPartVisual.CharacterPartBusyStates.FALLEN_ON_FLOOR)
        {
            AffectedCharacter.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
            RemoveSelf();
        }
    }
}
