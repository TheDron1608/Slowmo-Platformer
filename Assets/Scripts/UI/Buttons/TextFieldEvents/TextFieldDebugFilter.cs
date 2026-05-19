using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TextFieldDebugFilter : MonoBehaviour
{
    private string _oldFilter = "";

    public void Filter(string text)
    {
        string lowerText = text.ToLower();

        if (_oldFilter == lowerText) return;
        _oldFilter = lowerText;

        if (!GameObjectUtility.TryGetComponentInParentRecursive(transform, out DebugCardsManager cardsManager)) return;

        List<AbstractModificator> filteredMods =
            text != "" ?
            ModificatorDebugManager.Instance.DebugModificators
                .Where(e => e.Localization.TitleLocalization.StringReference.GetLocalizedString().ToLower().Contains(lowerText)).ToList() :
            ModificatorDebugManager.Instance.DebugModificators;

        if (filteredMods.Any(
            e1 => cardsManager.Cards.Any(
                e2 => e2 is ModificatorCardsCluster cluster && cluster.Cards.Any(
                    e3 => e3.ModificatorInstance == e1
            ))))
        {
            cardsManager.ClearAllCards();

            cardsManager.AddDebugCards(filteredMods);
        }
    }
}