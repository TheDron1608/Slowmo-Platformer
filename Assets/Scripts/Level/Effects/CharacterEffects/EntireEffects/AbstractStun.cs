public abstract class AbstractStun : AbstractOverwritingCharacterEffect
{
    protected override void OnRemove()
    {
        base.OnRemove();
        if (AffectedCharacter.CharacterEffectsReceiver.GetEffect<AbstractStun>() == null)
        {
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
    }
}