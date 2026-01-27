using System.Collections;
using UnityEngine;

public class Death : AbstractStun, ILethalEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();

        AffectedCharacter.CharacterEffectsReceiver.RemoveEffect<MinorStun>();

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

        AffectedCharacter.CharacterPartsManager.SetHitBoxHitableByProjectiles(false);

        if (AffectedCharacter.CharacterHolding.ThrowObjectsOnDeath)
        {
            AffectedCharacter.CharacterHolding.ForceStunThrow();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterPartsManager.SetHitBoxHitableByProjectiles(true);
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<Death>();
    }
}
