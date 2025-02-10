using System;
using UnityEngine;

public class CharacterComponentsManager : MonoBehaviour
{
    [Header("CharacterActions")]
    public CharacterMoving CharacterMoving;
    public CharacterJumping CharacterJumping;
    public CharacterInteractWithObjects CharacterInteract;
    public CharacterHoldingObjects CharacterHolding;
    public CharacterAiming CharacterAiming;
    public CharacterAttacking CharacterAttacking;
    public CharacterReloading CharacterReloading;
    public CharacterRolling CharacterRolling;
    [Header("CharacterInfo")]
    public CharacterCollisionInfo CharacterCollisionInfo;
    public CharacterVisual CharacterVisual;
    public CharacterInteractionWithTiles CharacterInteractionWithTiles;
    public GameObject Center;
    public GameObject CharacterPartsContainer;
    [Header("CharacterPhysics")]
    public Rigidbody2D CharacterRigidBody;
    public CapsuleCollider2D CharacterRigidBodyCapsuleCollider;

    public void SetIsAbleToDoAnyActions(bool freezeAllActions)
    {
        CharacterMoving.IsAbleToMove = freezeAllActions;
        CharacterJumping.IsAbleToJump = freezeAllActions;
        CharacterInteract.IsAbleToInteractWithObjects = freezeAllActions;
        CharacterHolding.IsAbleToGrabObjects = freezeAllActions;
        CharacterAiming.IsAbleToAim = freezeAllActions;
        CharacterAttacking.IsAbleToAttack = freezeAllActions;
        CharacterReloading.IsAbleToReload = freezeAllActions;
        CharacterRolling.IsAbleToRoll = freezeAllActions;
    }
}
