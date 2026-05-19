using System.Linq;
using UnityEngine;

public class HardUpModificatorOnPickNothingModificator : AbstractModificator
{
    public ModificatorStatuses AffectOnStatus;

    public override void OnModificatorChoiseFinished(AbstractModificatorCardsManager choise)
    {
        base.OnModificatorChoiseFinished(choise);

        if (choise.PickedModificators.Count == 0)
        {
            foreach (
                AbstractModificator validMod in 
                ModificatorsManager.Instance.CurrentModificators.Where(e => e.Status == AffectOnStatus && e.HarderVersion != null).ToArray()
                )
            {
                ModificatorsManager.Instance.RemoveModificator(validMod);
                ModificatorsManager.Instance.AddModificator(validMod.HarderVersion, validMod.Status);
                TryTriggerIconAnimation();
            }
        }
    }
}