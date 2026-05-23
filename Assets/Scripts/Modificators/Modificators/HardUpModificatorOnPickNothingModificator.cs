using System.Linq;
using UnityEngine;

public class HardUpModificatorOnPickNothingModificator : AbstractModificator
{
    public ModificatorStatuses AffectOnStatus;

    public override void OnModificatorChoiseFinished(AbstractModificatorCardsManager choise)
    {
        base.OnModificatorChoiseFinished(choise);

        if (choise.CardPickInfo.Any(e => e.Value && e.Key is ModificatorCardsCluster cluter && cluter.Cards.Count > 0))
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