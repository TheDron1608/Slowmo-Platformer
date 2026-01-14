using Unity.Mathematics;
using UnityEngine;

public class Adrenaline : AbstractCharacterEffect, IEntireCharacterEffect, IMultiplierableEffect
{
    const float SPEED_MULTIPLIER = 1f;
    const float JUMP_FORCE_MULTIPLIER = 0.33f;
    const float ROLL_MULTIPLIER = 1.25f;

    public float MaxSpeedMultiplier = 1.5f;

    private float _currentMultiplier = 1f;
    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    private void FixedUpdate()
    {
        AffectedCharacter.CharacterMoving.Speed /= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterMoving.SpeedAccelerationOnGroundMultiplier /= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterMoving.SpeedAccelerationOnAirMulitplier /= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterJumping.JumpForce /= _currentMultiplier * JUMP_FORCE_MULTIPLIER;
        AffectedCharacter.CharacterJumping.JumpOffWallForce /= _currentMultiplier * JUMP_FORCE_MULTIPLIER;
        AffectedCharacter.CharacterRolling.RollSpeed /= _currentMultiplier * ROLL_MULTIPLIER;

        _currentMultiplier = math.lerp(
            MaxSpeedMultiplier * EffectMultiplier, 
            1f,
            AffectedCharacter.CharacterHealth.MaxHealth > 0 ?
                math.min(AffectedCharacter.CharacterHealth.CurrentHealth, AffectedCharacter.CharacterHealth.MaxHealth) / AffectedCharacter.CharacterHealth.MaxHealth : 
                0f
            );

        AffectedCharacter.CharacterMoving.Speed *= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterMoving.SpeedAccelerationOnGroundMultiplier *= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterMoving.SpeedAccelerationOnAirMulitplier *= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterJumping.JumpForce *= _currentMultiplier * JUMP_FORCE_MULTIPLIER;
        AffectedCharacter.CharacterJumping.JumpOffWallForce *= _currentMultiplier * JUMP_FORCE_MULTIPLIER;
        AffectedCharacter.CharacterRolling.RollSpeed *= _currentMultiplier * ROLL_MULTIPLIER;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterMoving.Speed /= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterMoving.SpeedAccelerationOnGroundMultiplier /= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterMoving.SpeedAccelerationOnAirMulitplier /= _currentMultiplier * SPEED_MULTIPLIER;
        AffectedCharacter.CharacterJumping.JumpForce /= _currentMultiplier * JUMP_FORCE_MULTIPLIER;
        AffectedCharacter.CharacterJumping.JumpOffWallForce /= _currentMultiplier * JUMP_FORCE_MULTIPLIER;
        AffectedCharacter.CharacterRolling.RollSpeed /= _currentMultiplier * ROLL_MULTIPLIER;
    }
}
