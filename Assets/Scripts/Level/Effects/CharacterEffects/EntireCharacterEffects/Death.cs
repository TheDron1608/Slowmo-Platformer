using System.Collections;
using UnityEngine;

public class Death : AbstractStun, ILethalEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();

        AffectedObject.RemoveEffectExceptSelf(this);

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

        AffectedCharacter.CharacterAIManager?.SetAIDisabled(true);

        if (AffectedCharacter.CharacterSpecial != null)
        {
            AffectedCharacter.CharacterSpecial.IsAbleToDoSpecial = false;
        }

        AffectedCharacter.CharacterPartsManager.SetHitBoxHitableByProjectiles(false);

        if (AffectedCharacter.CharacterHolding.ThrowObjectsOnDeath)
        {
            AffectedCharacter.CharacterHolding.ForceStunThrow();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterAIManager?.SetAIDisabled(false);

        AffectedCharacter.CharacterPartsManager.SetHitBoxHitableByProjectiles(true);
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && !affectWho.GetHasEffect<Death>();
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        
    }
}
