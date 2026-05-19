using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ButtonOnClickFilterDebugModificatorPick : MonoBehaviour
{
    public List<AbstractModificator.ModificatorTypes> FilterType = new();

    public void Filter()
    {
        if (!GameObjectUtility.TryGetComponentInParentRecursive(transform, out DebugCardsManager cardsManager)) return;

        cardsManager.ClearAllCards();

        cardsManager.AddDebugCards(ModificatorDebugManager.Instance.DebugModificators.Where(e => FilterType.Contains(e.ModificatorType)).ToList());
    }
}