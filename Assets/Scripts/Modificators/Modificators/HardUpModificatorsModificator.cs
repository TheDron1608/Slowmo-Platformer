using System.Collections.Generic;

class HardUpModificatorsModificator : AbstractModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        List<AbstractModificator> addedMods = new();

        for (int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
        {
            AbstractModificator currentMod = ModificatorsManager.Instance.CurrentModificators[i];
            AbstractModificator harderMod = currentMod.HarderVersion;
            if (harderMod != null && !addedMods.Contains(currentMod))
            {
                ModificatorsManager.Instance.RemoveModificator(currentMod);
                addedMods.Add(ModificatorsManager.Instance.AddModificator(harderMod, currentMod.Status));
                i--;
            }
        }
    }
}