using UnityEngine;

public class Busy : AbstractStun
{
    protected override void OnApply()
    {
        base.OnApply();

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

    public override bool ApplyCondition(CharacterComponentsManager affectWho)
    {
        return base.ApplyCondition(affectWho) && !affectWho.CharacterEffects.GetHasEffect<AbstractStun>();
    }
}
