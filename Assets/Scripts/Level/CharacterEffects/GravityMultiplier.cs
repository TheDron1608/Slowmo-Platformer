using UnityEngine;

public class GravityMultiplier : AbstractCharacterEffect
{
    public float GravityMultiplierAmount = 1f;

    protected override void OnApply()
    {
        base.OnApply();
        AffectedCharacter.CharacterRigidBody.gravityScale *= GravityMultiplierAmount;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        AffectedCharacter.CharacterRigidBody.gravityScale /= GravityMultiplierAmount;
    }

    public override bool Equals(AbstractCharacterEffect other)
    {
        return base.Equals(other) && GravityMultiplierAmount == (other as GravityMultiplier).GravityMultiplierAmount;
    }
}
