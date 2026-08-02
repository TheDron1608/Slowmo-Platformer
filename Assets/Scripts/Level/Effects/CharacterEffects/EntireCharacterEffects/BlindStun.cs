using UnityEngine;

public class BlindStun : AbstractStun, IMultiplierableEffect
{
    const float SCREEN_OVERLAY_CONSTANT_OVERLAY_AMOUNT = 0.25f;

    public float StunDuration = 5f;
    public float PlayerStunDuration = 5f;

    private Texture2D viewportLastSeenTexture = null;
    private float _stunTime = 0f;
    private float _effectMultiplier = 1f;
    private bool _affectedIsPlayer;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        _affectedIsPlayer =
            (Camera.main?.TryGetComponent(out CameraTrack tracker) ?? false) &&
            tracker.TrackTargets.Contains(AffectedObject.transform) &&
            UIManager.Instance != null;

        if (_affectedIsPlayer)
        {
            UIManager.Instance.BlindnessOverlay.Show(
                StunDuration * EffectMultiplier * SCREEN_OVERLAY_CONSTANT_OVERLAY_AMOUNT,
                StunDuration * EffectMultiplier * (1f - SCREEN_OVERLAY_CONSTANT_OVERLAY_AMOUNT)
                );
        }
        else
        {
            base.OnApply();

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
            AffectedCharacter.CharacterVisual.PopupStunned();
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (AffectedCharacter.TryGetComponent(out CharacterUITrack trackedCharacter) && UIManager.Instance != null)
        {
            UIManager.Instance.BlindnessOverlay.Hide();
        }
       
        if (!_affectedIsPlayer) AffectedCharacter.CharacterVisual.RemovePopupStunned();
    }

    private void FixedUpdate()
    {
        _stunTime += Time.deltaTime;

        if (
            AffectedCharacter.CharacterHealth.Died ||
            _stunTime > (_affectedIsPlayer ? PlayerStunDuration : StunDuration) * EffectMultiplier
            )
        {
            RemoveSelf();
        }
    }
}