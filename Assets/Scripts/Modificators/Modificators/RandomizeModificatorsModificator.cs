using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomizeModificatorsModificator : AbstractModificator
{
    const float MAX_MODIFICATOR_PRICE_CHANGE = 1.1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        List<AbstractModificator> oldModOriginals = new();
        List<ModificatorStatuses> oldModStatuses = new();

        //removing old modificators
        for (int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
        {
            AbstractModificator mod = ModificatorsManager.Instance.CurrentModificators[i];
            if (mod != this && mod.Status != ModificatorStatuses.CHARACTER_DEFAULT && mod.ModificatorTier != ModificatorTiers.TIER_ULTRA)
            {
                oldModOriginals.Add(mod.OriginalModificator);
                oldModStatuses.Add(mod.Status);

                ModificatorsManager.Instance.RemoveModificatorAt(i);
                i--;
            }
        }

        //adding new modificators
        for (int i = 0; i < oldModOriginals.Count; i++)
        {
            AbstractModificator newModificator = ModificatorsManager.Instance.PickRandomModificators(
                oldModOriginals[i].ModificatorType,
                oldModOriginals[i].ModificatorPrice / MAX_MODIFICATOR_PRICE_CHANGE,
                oldModOriginals[i].ModificatorPrice * MAX_MODIFICATOR_PRICE_CHANGE,
                false,
                false,
                false,
                new List<AbstractModificator> { oldModOriginals[i] },
                true
                ).FirstOrDefault();

            if (newModificator != null)
            {
                ModificatorsManager.Instance.AddModificator(newModificator, oldModStatuses[i]);
            }
            else if (ModificatorsManager.Instance.CurrentModificators.All(e => !e.GetIsRestrictedWith(oldModOriginals[i])))
            {
                ModificatorsManager.Instance.AddModificator(oldModOriginals[i], oldModStatuses[i]);
            }
        }
    }
}