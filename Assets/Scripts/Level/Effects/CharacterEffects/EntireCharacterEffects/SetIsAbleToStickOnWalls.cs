
public class SetIsAbleToStickOnWalls : AbstractCharacterEffect, IEntireCharacterEffect
{
    public bool Value;

    private bool _oldIsAbleToStickOnWalls;

    protected override void OnApply()
    {
        base.OnApply();

        _oldIsAbleToStickOnWalls = AffectedCharacter.CharacterInteractionWithTiles.IsAbleToStickOnWalls;
        AffectedCharacter.CharacterInteractionWithTiles.IsAbleToStickOnWalls = Value;
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        AffectedCharacter.CharacterInteractionWithTiles.IsAbleToStickOnWalls = _oldIsAbleToStickOnWalls;
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && Value == (other as SetIsAbleToStickOnWalls).Value;
    }
}