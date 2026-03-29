using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterFinishOff : AbstractCharacterSpecial
{
    const float MOVE_TO_FINISH_OFF_SPEED = 10f;
    const float HARD_STUN_RECOVER_TIME_MULTIPLIER_ON_FINIHS = 0.1f;

    public List<AbstractEffect> EffectOnFinish = new();
    public float MaxDistanceForFinish = 1f;
    public CharacterVisual.CharacterPartBusyStates FinishAnimation = CharacterVisual.CharacterPartBusyStates.FINISH_OFF;
    public CharacterPart.PartTypes FinishAffectedLimb = CharacterPart.PartTypes.HEAD;

    private bool _isFinishingOff = false;
    private AbstractCharacterComponent _currentFinishingCharacter = null;

    public bool TryFinishOff(AbstractCharacterComponent character)
    {
        if (FinishOffCondition(character) && !_isFinishingOff)
        {
            StartCoroutine(FinishOffCoroutine(character));

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool FinishOffCondition(AbstractCharacterComponent character)
    {
        return 
            Vector2.Distance(CharComponents.Center.transform.position, character.CharComponents.Center.transform.position) < MaxDistanceForFinish &&
            CharComponents.CharacterCollision.CurrentZLayer == character.CharComponents.CharacterCollision.CurrentZLayer &&
            !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharComponents.CharacterTeam) &&
            character.CharComponents.CharacterPartsManager.GetCharacterPart(FinishAffectedLimb) is CharacterLimbPart &&
            character.CharComponents.CharacterCollision.IsCollidingFloor() &&
            character.CharComponents.CharacterEffectsReceiver.GetHasEffect<HardStun>();
    }

    private IEnumerator FinishOffCoroutine(AbstractCharacterComponent character)
    {
        _isFinishingOff = true;
        _currentFinishingCharacter = character;

        character.CharComponents.CharacterRigidBody.linearVelocity = Vector2.zero;
        character.CharComponents.CharacterVisual.StunRecoverAnimationTimeMult *= HARD_STUN_RECOVER_TIME_MULTIPLIER_ON_FINIHS;

        CharComponents.CharacterRigidBody.linearVelocity = Vector2.zero;
        CharComponents.CharacterVisual.CurrentBusyAnimation = FinishAnimation;

        while (CharComponents.CharacterVisual.CurrentBusyAnimation == FinishAnimation)
        {
            if (!FinishOffCondition(character))
            {
                CharComponents.CharacterVisual.BreakBusyAnimation();
                break;
            }

            Bounds finishCharacterLimbColliderBounds = 
                (character.CharComponents.CharacterPartsManager.GetCharacterPart(FinishAffectedLimb) as CharacterLimbPart).Collider.bounds;
            Vector2 finishPositionRelativeToCharacterPosition =
                CharComponents.FinishOffPosition.transform.position - CharComponents.transform.position;
            Vector3 targetFinishPosition = new(
                finishCharacterLimbColliderBounds.center.x - finishPositionRelativeToCharacterPosition.x,
                finishCharacterLimbColliderBounds.min.y - finishPositionRelativeToCharacterPosition.y,
                CharComponents.transform.position.z
                );

            CharComponents.transform.position = math.lerp(CharComponents.transform.position, targetFinishPosition, Time.deltaTime * MOVE_TO_FINISH_OFF_SPEED);

            yield return new WaitForEndOfFrame();
        }

        character.CharComponents.CharacterVisual.StunRecoverAnimationTimeMult /= HARD_STUN_RECOVER_TIME_MULTIPLIER_ON_FINIHS;

        _currentFinishingCharacter = null;
        _isFinishingOff = false;
    }

    public void Animator_FinishFinishingOff()
    {
        if (
            _currentFinishingCharacter != null && 
            !_currentFinishingCharacter.IsDestroyed() &&
            _currentFinishingCharacter.CharComponents.CharacterPartsManager.GetCharacterPart(FinishAffectedLimb) is CharacterLimbPart finishLimb
            )
        {
            finishLimb.CharPartEffectsReceiver.ApplyEffect(EffectOnFinish, CharComponents.CharacterAttacking);
        }
    }
}