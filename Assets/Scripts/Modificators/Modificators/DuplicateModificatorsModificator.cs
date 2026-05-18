using System.Collections.Generic;
using Unity.VisualScripting;

public class DuplicateModificatorsModificator : AbstractModificator
{
    public ModificatorStatuses AffectOnStatus;

    private List<AbstractModificator> _clonedModificators = new();

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        List<AbstractModificator> cloneModificators = new();

        foreach (AbstractModificator mod in ModificatorsManager.Instance.CurrentModificators)
        {
            if (mod.Stackable && mod.Status == AffectOnStatus && !(mod is DuplicateModificatorsModificator))
            {
                cloneModificators.Add(mod.OriginalModificator);
            }
        }

        foreach (AbstractModificator cloneMod in cloneModificators)
        {
            _clonedModificators.Add(ModificatorsManager.Instance.AddModificator(cloneMod, AffectOnStatus));
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            foreach (AbstractModificator clonedMod in _clonedModificators)
            {
                if (clonedMod != null && !clonedMod.IsDestroyed())
                {
                    ModificatorsManager.Instance.RemoveModificator(clonedMod);
                }
            }
        }
    }
}