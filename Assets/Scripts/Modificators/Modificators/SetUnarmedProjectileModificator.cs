
public class SetUnarmedProjectileModificator : AbstractCharactersModificator
{
    public AbstractProjectile UnarmedProjectile;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.UnarmedAttacking.Projectile = UnarmedProjectile;
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.UnarmedAttacking.Projectile = null;
    }
}