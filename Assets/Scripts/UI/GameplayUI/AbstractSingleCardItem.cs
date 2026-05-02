using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractSingleCardItem : AbstractCardItem, IModificatorInfo
{
    [SerializeField] private ModificatorLocalization _localization;

    public ModificatorLocalization Localization
    {
        get => _localization;
    }

    public AbstractModificator.ModificatorStatuses Status => AbstractModificator.ModificatorStatuses.NONE;

    public bool DisabledModificator => false;

    public float ModificatorPrice => 0;

    public bool Multiplierable => false;

    public float ModificatorMultiplier => 1f;

    public float? GetSpoilProgress()
    {
        return null;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.SetDisplayedInfo(new List<IModificatorInfo>() { this });
        }
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (modificator.CurrentIcon != null)
            {
                modificator.CurrentIcon.Raising = false;
                modificator.CurrentIcon.DisabledModificator = modificator.DisabledModificator;
            }
        }
    }

    protected override void OnValidate()
    {
        _localization = transform.GetComponentInChildren<ModificatorLocalization>();
    }
}