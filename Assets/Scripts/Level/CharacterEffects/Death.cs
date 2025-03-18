using System.Collections;
using UnityEngine;

public class Death : AbstractCharacterEffect
{
    private bool _diedThisFrame = true;

    public bool DiedThisFrame
    {
        get => _diedThisFrame; 
        private set => _diedThisFrame = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();

        AffectedCharacter.CharacterEffects.RemoveEffect<MinorStun>();

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

        AffectedCharacter.CharacterPartsManager.SetHitBoxHitableByProjectiles(false);

        StartCoroutine(AwaitFrameThenSetDiedThisFrame());
    }

    private IEnumerator AwaitFrameThenSetDiedThisFrame()
    {
        yield return new WaitForEndOfFrame();
        _diedThisFrame = false;
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

        AffectedCharacter.CharacterPartsManager.SetHitBoxHitableByProjectiles(true);
    }
}
