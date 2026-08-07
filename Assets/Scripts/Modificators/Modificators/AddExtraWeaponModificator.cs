
public class AddExtraWeaponModificator : AbstractModificator
{
    public EnemyWeaponInfo AddExtraWeapon;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        SpawnManager.Instance.ExtraWeaponPool.Add(AddExtraWeapon);
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        SpawnManager.Instance?.ExtraWeaponPool.Remove(AddExtraWeapon);
    }
}