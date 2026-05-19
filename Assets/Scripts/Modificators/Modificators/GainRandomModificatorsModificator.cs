
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GainRandomModificatorsModificator : AbstractModificator
{
    public ModificatorTypes Type;
    public int Amount = 1;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        for (int i = 0; i < Amount; i++)
        {
            AbstractModificator newMod = ModificatorsManager.Instance.PickRandomModificators(
                Type,
                0f,
                float.MaxValue,
                false,
                false,
                false,
                new List<AbstractModificator>() { OriginalModificator },
                true
                ).FirstOrDefault();

            if (newMod != null )
            {
                ModificatorsManager.Instance.AddModificator(newMod, Status);
            }
            else
            {
                break;
            }
        }
    }
}