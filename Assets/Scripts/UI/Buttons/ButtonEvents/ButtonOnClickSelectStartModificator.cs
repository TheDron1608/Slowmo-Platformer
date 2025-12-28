using System.Collections.Generic;
using UnityEngine;

public class ButtonOnClickSelectStartModificator : MonoBehaviour
{
    public List<AbstractModificator> Modificators;

    public void SelectModificator()
    {
        foreach (AbstractModificator mod in Modificators)
        {
            ModificatorsManager.Instance.AddModificator(mod);
        }
    }
}
