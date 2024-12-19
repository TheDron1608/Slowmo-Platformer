using System;
using UnityEngine;

public class CharacterActions : MonoBehaviour
{
    public CharacterMoving CharacterMovingAction;
    public CharacterJumping CharacterJumpingAction;
    public CharacterInteractWithObjects CharacterInteractAction;

    public void SetIsAbleToDoAnyActions(bool freezeAllActions)
    {
        CharacterMovingAction.IsAbleToMove = freezeAllActions;
        CharacterJumpingAction.IsAbleToJump = freezeAllActions;
        CharacterInteractAction.IsAbleToInteractWithObjects = freezeAllActions;
    }
}
