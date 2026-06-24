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
    public AbstractCharacterSpecial CharacterSpecial;
    [Header("CharacterInfo")]
    public CharacterTeam CharacterTeam;
    public CharacterAIManager CharacterAIManager;
    public CharacterCollision CharacterCollision;
    public CharacterVisual CharacterVisual;
    public CharacterInteractionWithTiles CharacterInteractionWithTiles;
    public CharacterEffectsReceiver CharacterEffectsReceiver;
    public CharacterStuckedObjects CharacterStuckedObjects;
    public CharacterHealth CharacterHealth;
    public CharacterClumsyness CharacterClumsyness;
    public CharacterPartsManager CharacterPartsManager;
    public Transform NavPointsContainer;
    public CharacterPositionPoint Center;
    public CharacterPositionPoint Bottom;
    public CharacterPositionPoint FinishOffPosition;
    public GameObject CharacterPartsContainer;
    public Animator Animator;
    public SpriteRenderer SampleSpriteRenderer;
    public CharacterLoseLimbParticleSpawner LoseLimbParticleSpawner;
    public CharacterUITrack UITrack;
    [Header("CharacterPhysics")]
    public Rigidbody2D CharacterRigidBody;
    public CapsuleCollider2D CharacterRigidBodyCapsuleCollider;
    public CharacterHitbox CharacterRigidBodyCapsuleColliderHitBox;
}
