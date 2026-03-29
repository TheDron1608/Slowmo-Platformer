public class HardStun : AbstractStun, IMultiplierableEffect
{
    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterEffectsReceiver.RemoveEffect<MinorStun>();

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
