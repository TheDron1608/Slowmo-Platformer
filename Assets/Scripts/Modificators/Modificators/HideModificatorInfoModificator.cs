using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;

public class HideModificatorInfoModificator : AbstractModificator
{
    public bool HideTitle = false;
    public bool HideDescription = true;
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
            card.Localization.HideTitle = HideTitle;
            card.Localization.HideDescription = HideDescription;
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
            card.Localization.HideTitle = false;
            card.Localization.HideDescription = false;
            card.OverrideSprite = null;
        }
    }
}