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
        AffectedCharacter.CharacterInteractionWithTiles.IsCurrentAbleToStickOnWalls = false;
        if (AffectedCharacter.CharacterSpecial != null)
        {
            AffectedCharacter.CharacterSpecial.IsAbleToDoSpecial = false;
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) && 
            !affectWho.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterEffectsReceiver.GetHasEffect<AbstractStun>();
    }
}
