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
    public CharacterDamaging CharacterDamaging;
    public CharacterEffects CharacterEffects;
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
        CharacterHolding.IsAbleToThrowObjects = freezeAllActions;
        CharacterHolding.IsAbleToHoldObjects = freezeAllActions;
        CharacterAiming.IsAbleToAim = freezeAllActions;
        CharacterAttacking.IsAbleToAttack = freezeAllActions;
        CharacterAttacking.IsAbleToHammer = freezeAllActions;
        CharacterAttacking.IsAbleToStartChainsaw = freezeAllActions;
        CharacterReloading.IsAbleToReload = freezeAllActions;
        CharacterRolling.IsAbleToRoll = freezeAllActions;
        CharacterInteractionWithTiles.IsAbleToStickOnWalls = freezeAllActions;
    }
}
