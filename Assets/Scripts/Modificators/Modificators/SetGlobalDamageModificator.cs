using UnityEngine;

public class SetGlobalDamageModificator : AbstractModificator
{
    public float GlobalDamageMultiplier = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        DamageManager.Instance.GlobalDamageMultiplier *= GlobalDamageMultiplier;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (DamageManager.Instance != null)
        {
            DamageManager.Instance.GlobalDamageMultiplier /= GlobalDamageMultiplier;
        }
    }
}