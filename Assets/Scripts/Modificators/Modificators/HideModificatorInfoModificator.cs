using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

public class HideModificatorInfoModificator : AbstractModificator
{
    const char REPLACE_CHAR = '?';

    public Sprite HiddenSprite;
    private List<ModificatorCard> _affectedCards = new();

    public override void OnModificatorChoiseStarted()
    {
        base.OnModificatorChoiseStarted();

        CursePickManager.Instance.OnAddedItem += Instance_OnAddedItem;
    }

    private void Instance_OnAddedItem(object sender, ModificatorCardsCluster e)
    {
        foreach (ModificatorCard card in e.Cards)
        {
            foreach (LocalizeStringEvent localization in card.GetComponents<LocalizeStringEvent>())
            {
                localization.enabled = false;
            }
            card.LocalizedTitle = card.LocalizedTitle.FilterReplace(REPLACE_CHAR, false, false, true, true, true, true);
            card.LocalizedDescription = card.LocalizedDescription.FilterReplace(REPLACE_CHAR, false, false, true, true, true, true);
            card.OverrideSprite = HiddenSprite;
            _affectedCards.Add(card);
        }
    }

    public override void OnModificatorChoiseFinished()
    {
        base.OnModificatorChoiseFinished();

        CursePickManager.Instance.OnAddedItem -= Instance_OnAddedItem;
        _affectedCards = new();
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        foreach (ModificatorCard card in _affectedCards)
        {
            foreach (LocalizeStringEvent localization in card.GetComponents<LocalizeStringEvent>())
            {
                localization.enabled = true;
                localization.RefreshString();
            }
            card.OverrideSprite = null;
        }
    }
}