using System.Collections.Generic;
using UnityEngine;

public class HardStun : AbstractStun, IMultiplierableEffect
{
    private float _effectMultiplier = 1f;
    private List<AbstractCharacterComponent> _totalStunSenderCharacters = new();
    private HardStun _oldStun = null;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    public List<AbstractCharacterComponent> TotalStunSenderCharacters
    {
        get => _totalStunSenderCharacters;
    }

    protected override void OnApply()
    {
        _oldStun = transform.parent.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterEffectsReceiver.GetEffect<HardStun>();
        AffectedObject.RemoveEffect<MinorStun>();

        base.OnApply();

        AffectedCharacter.CharacterVisual.BreakBusyAnimation();
        AffectedCharacter.CharacterVisual.StunRecoverAnimationTimeMult /= EffectMultiplier;
        AffectedCharacter.CharacterVisual.CurrentBusyAnimation = CharacterVisual.CharacterPartBusyStates.FALLING_IN_AIR;
        AffectedCharacter.CharacterVisual.OnBusyStateChanged += CharacterVisual_OnBusyStateChanged;

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
        if (AffectedCharacter.CharacterSpecial != null)
        {
            AffectedCharacter.CharacterSpecial.IsAbleToDoSpecial = false;
        }

        if (AffectedCharacter.CharacterHolding.ThrowObjectsOnStun)
        {
            AffectedCharacter.CharacterHolding.ForceStunThrow();
        }
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        AbstractCharacterComponent characterSender = ObjectEffectsReceiver.TryGetCharacterFromSender(sender);
        if (characterSender != null)
        {
            _totalStunSenderCharacters.Add(characterSender);

            if (
                characterSender.CharComponents.CharacterEffectsReceiver.TryGetEffect(out HardStun senderStun) &&
                senderStun.TotalStunSenderCharacters.Count > 0
                )
            {
                _totalStunSenderCharacters.AddRange(senderStun.TotalStunSenderCharacters);
            }
        }

        List<AbstractCharacterComponent> oldStunSenderCharacters = _oldStun?.TotalStunSenderCharacters;
        if (oldStunSenderCharacters != null && oldStunSenderCharacters.Count > 0)
        {
            _totalStunSenderCharacters.AddRange(oldStunSenderCharacters);
        }
    }

    private void CharacterVisual_OnBusyStateChanged(object sender, CharacterVisual.OnBusyStateChangedEventArgs e)
    {
        if (e.NewState != CharacterVisual.CharacterPartBusyStates.FALLING_IN_AIR && e.OldState == CharacterVisual.CharacterPartBusyStates.FALLEN_ON_FLOOR)
        {
            AffectedCharacter.CharacterVisual.OnBusyStateChanged -= CharacterVisual_OnBusyStateChanged;
            RemoveSelf();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        AffectedCharacter.CharacterVisual.StunRecoverAnimationTimeMult *= EffectMultiplier;
    }
}
