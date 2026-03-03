public class ComboSpeed : AbstractCharacterEffect, IEntireCharacterEffect, IMultiplierableEffect
{
    public float MoveSpeedPerCombo = 0.05f;
    public float JumpForcePerCombo = 0.01f;

    private float _moveMult = 1f;
    private float _jumpMult = 1f;   
    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    private void FixedUpdate()
    {
        float newMoveMult = 1f + (ScoreManager.Instance?.CurrentCombo ?? 0) * EffectMultiplier * MoveSpeedPerCombo;
        if (_moveMult != newMoveMult)
        {
            AffectedCharacter.CharacterMoving.Speed = AffectedCharacter.CharacterMoving.Speed * newMoveMult / _moveMult;
            _moveMult = newMoveMult;
        }

        float newJumpMult = 1f + (ScoreManager.Instance?.CurrentCombo ?? 0) * EffectMultiplier * JumpForcePerCombo;
        if (_jumpMult != newMoveMult)
        {
            AffectedCharacter.CharacterJumping.JumpForce = AffectedCharacter.CharacterJumping.JumpForce * newJumpMult / _jumpMult;
            _jumpMult = newJumpMult;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterMoving.Speed /= _moveMult;
        AffectedCharacter.CharacterJumping.JumpForce /= _jumpMult;
    }
}
