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
    public UnarmedWeapon UnarmedAttacking;
    [Header("CharacterInfo")]
    public CharacterCollision CharacterCollisionInfo;
    public CharacterVisual CharacterVisual;
    public CharacterInteractionWithTiles CharacterInteractionWithTiles;
    public CharacterDamaging CharacterDamaging;
    public CharacterEffects CharacterEffects;
    public CharacterStuckedObjects CharacterStuckedObjects;
    public CharacterHealth CharacterHealth;
    public CharacterClumsyness CharacterClumsyness;
    public CharacterPartsManager CharacterPartsManager;
    public CharacterPositionPoint Center;
    public CharacterPositionPoint Bottom;
    public GameObject CharacterPartsContainer;
    public Animator Animator;
    public SpriteRenderer SampleSpriteRenderer;
    [Header("CharacterPhysics")]
    public Rigidbody2D CharacterRigidBody;
    public CapsuleCollider2D CharacterRigidBodyCapsuleCollider;
    public CharacterHitbox CharacterRigidBodyCapsuleColliderHitBox;
}
