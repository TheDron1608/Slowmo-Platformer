
public class SetUnarmedProjectileModificator : AbstractCharactersModificator
{
    public AbstractProjectile UnarmedProjectile;
    public AbstractProjectile AltUnarmedProjectileOnInvert = null;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.UnarmedAttacking.Projectile = InvertTeam && AltUnarmedProjectileOnInvert != null ? AltUnarmedProjectileOnInvert : UnarmedProjectile;
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.UnarmedAttacking.Projectile = null;
    }
}