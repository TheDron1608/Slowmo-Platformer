using System;
using UnityEngine;

public class CharacterActions : MonoBehaviour
{
    public CharacterMoving CharacterMovingAction;
    public CharacterJumping CharacterJumpingAction;
    public CharacterInteractWithObjects CharacterInteractAction;
    public CharacterHoldingObjects CharacterHoldingAction;
    public CharacterAiming CharacterAimingAction;
    public CharacterAttacking CharacterAttackingAction;
    public CharacterReloading CharacterReloadingAction;
    public CharacterRolling CharacterRollingAction;

    public void SetIsAbleToDoAnyActions(bool freezeAllActions)
    {
        CharacterMovingAction.IsAbleToMove = freezeAllActions;
        CharacterJumpingAction.IsAbleToJump = freezeAllActions;
        CharacterInteractAction.IsAbleToInteractWithObjects = freezeAllActions;
        CharacterHoldingAction.IsAbleToGrabObjects = freezeAllActions;
        CharacterAimingAction.IsAbleToAim = freezeAllActions;
        CharacterAttackingAction.IsAbleToAttack = freezeAllActions;
        CharacterReloadingAction.IsAbleToReload = freezeAllActions;
        CharacterRollingAction.IsAbleToRoll = freezeAllActions;
    }
}
