public class AddAirJumps : AbstractCharacterEffect, IEntireCharacterEffect
{
    public int AddJumps = 0;

    protected override void OnApply()
    {
        base.OnApply();
        AffectedCharacter.CharacterJumping.AirJumps += AddJumps;
        AffectedCharacter.CharacterJumping.AirJumpsLeft += AddJumps;
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        AffectedCharacter.CharacterJumping.AirJumps -= AddJumps;
        AffectedCharacter.CharacterJumping.AirJumpsLeft -= AddJumps;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && AddJumps == (other as AddAirJumps).AddJumps;
    }
}
