using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ModificatorsManager : MonoBehaviour
{
    const int MULTIPLE_MODIFICATORS_ORDER_LIMIT = 10;
    const int MULTIPLE_MODIFICATORS_MAX_AMOUNT = 5;
    const float MIN_SINGLE_MODIFICATOR_REQUIRED_PRICE = 0.75f;

    public static ModificatorsManager Instance;

    public List<AbstractModificator> ModificatorsPool = new();
    public int MaxModificatorOptions = 3;
    public int ModifiactorsPickAmount = 1;
    public int BlessPickRerolls = 0;
    public int CursePickRerolls = 0;
    public int DifficultyCursePickRerolls = 0;
    public int DifficultyUpNegativeModificatorsPickAmount = 1;
    public float ExtraModificatorChance = 0.1f;
    public float ExtraNeutralModificatorChance = 0.1f;
    public bool CanSkipBlessPick = true;
    public bool CanSkipCursePick = true;
    public bool CanSellCurses = true;
    public bool CanPickCurses = true;
    public bool RemoveModifictorsOnSell = true;
    public bool ResetScoreOnSell = true;
    public float TradeCurseProfitMult = 1f;
    public float TradeBlessProfitMult = 1f;
    public float ArtifactSpoilDurationSeconds = 60 * 5f;

    [Header("Instances")]
    [SerializeField] private ModificatorCardsCluster _clusterInstance;
    [SerializeField] private ModificatorIcon _emptyIconIstance;
    [SerializeField] private ModificatorCard _emptyCardIstance;

    [SerializeField] private List<Sprite> _characterIconBg;
    [SerializeField] private List<Sprite> _permanentIconBg;
    [SerializeField] private List<Sprite> _tradableIconBg;
    [SerializeField] private List<Sprite> _artifactIconBg;

    [SerializeField] private List<Material> _positiveCardTierMaterials = new();
    [SerializeField] private List<Material> _negativeCardTierMaterials = new();
    [SerializeField] private List<Material> _neutralCardTierMaterials = new();

    private List<AbstractModificator> _currentModificators = new();
    private List<AbstractModificator> _avaibleValidModificators = new();
    private List<AbstractModificator> _avaibleSynergingValidModificators = new();
    private List<AbstractModificator> _avaibleUnsynergingValidModificators = new();
    private bool _requestUpdateAvaibleModificators = true;

    public List<AbstractModificator> CurrentModificators
    {
        get => _currentModificators;
    }
    public List<AbstractModificator> AvaibleValidModificators
    {
        get
        {
            if (_requestUpdateAvaibleModificators) UpdateAvaibleModificatorsInfo();
            return _avaibleValidModificators;
        }
    }
    public List<AbstractModificator> AvaibleSynergingValidModificators
    {
        get
        {
            if (_requestUpdateAvaibleModificators) UpdateAvaibleModificatorsInfo();
            return _avaibleSynergingValidModificators;
        }
    }
    public List<AbstractModificator> AvaibleUnsynergingValidModificators
    {
        get
        {
            if (_requestUpdateAvaibleModificators) UpdateAvaibleModificatorsInfo();
            return _avaibleUnsynergingValidModificators;
        }
    }

    private void UpdateAvaibleModificatorsInfo()
    {
        _avaibleValidModificators = ModificatorsPool.Where(
            poolMod => CurrentModificators.All(
                curMod => !poolMod.GetIsRestrictedWith(curMod)
                )
            ).ToList();

        _avaibleSynergingValidModificators = _avaibleValidModificators.Where(
            validMod => CurrentModificators.Any(
                curMod => curMod.GetIsSynergingWith(validMod)
                )
            ).ToList();

        _avaibleUnsynergingValidModificators = _avaibleValidModificators.Where(
            validMod => CurrentModificators.Any(
                curMod => curMod.GetIsUnsynergingWith(validMod)
                )
            ).ToList();

        _requestUpdateAvaibleModificators = false;
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 ModificatorsManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ModificatorIcon CreateModificatorIcon(AbstractModificator modificator, Transform parent)
    {
        ModificatorIcon result = Instantiate(_emptyIconIstance, parent);
        result.name = modificator.name + "Icon";
        result.CurrentModificator = modificator;
        result.ModificatorInstance = modificator.OriginalModificator;
        result.DisabledModificator = modificator.DisabledModificator;

        if (modificator.CustomIconContent == null)
        {
            result.TitleImage.sprite = modificator.IconSprite;
            result.CustomContentContainer.gameObject.SetActive(false);
        }
        else
        {
            Instantiate(modificator.CustomIconContent, result.CustomContentContainer);
            result.TitleImage.gameObject.SetActive(false);
        }

        switch (modificator.ModificatorType)
        {
            case AbstractModificator.ModificatorTypes.POSITIVE:
                result.BgImage.material = _positiveCardTierMaterials[(int)modificator.ModificatorTier];
                break;
            case AbstractModificator.ModificatorTypes.NEGATIVE:
                result.BgImage.material = _negativeCardTierMaterials[(int)modificator.ModificatorTier];
                break;
            case AbstractModificator.ModificatorTypes.NEUTRAL:
                result.BgImage.material = _neutralCardTierMaterials[(int)modificator.ModificatorTier];
                break;
        }

        switch (modificator.Status)
        {
            case AbstractModificator.ModificatorStatuses.CHARACTER_DEFAULT:
                result.BgImage.sprite = NumberMath.PickRandomItem(_characterIconBg);
                break;
            case AbstractModificator.ModificatorStatuses.PERMANENT:
            case AbstractModificator.ModificatorStatuses.TRADED:
                result.BgImage.sprite = NumberMath.PickRandomItem(_permanentIconBg);
                break;
            case AbstractModificator.ModificatorStatuses.CURSE:
                result.BgImage.sprite = NumberMath.PickRandomItem(_tradableIconBg);
                break;
            case AbstractModificator.ModificatorStatuses.ARTIFACT:
                result.BgImage.sprite = NumberMath.PickMiddleItem(_artifactIconBg);
                break;
        }

        return result;
    }

    public ModificatorCard CreateModificatorCard(AbstractModificator modificator, Transform parent)
    {
        ModificatorCard result = Instantiate(_emptyCardIstance, parent);
        result.name = modificator.name + "Card";
        result.ModificatorInstance = modificator;

        switch (modificator.ModificatorType)
        {
            case AbstractModificator.ModificatorTypes.POSITIVE:
                result.BgImage.material = _positiveCardTierMaterials[(int)modificator.ModificatorTier];
                break;
            case AbstractModificator.ModificatorTypes.NEGATIVE:
                result.BgImage.material = _negativeCardTierMaterials[(int)modificator.ModificatorTier];
                break;
            case AbstractModificator.ModificatorTypes.NEUTRAL:
                result.BgImage.material = _neutralCardTierMaterials[(int)modificator.ModificatorTier];
                break;
        }

        if (modificator.CustomCardContent == null)
        {
            result.TitleImage.sprite = modificator.CardSprite;
            result.CustomContentContainer.gameObject.SetActive(false);
        }
        else
        {
            Instantiate(modificator.CustomCardContent, result.CustomContentContainer);
            result.TitleImage.gameObject.SetActive(false);
        }

        result.Localization = Instantiate(modificator.Localization, result.transform);

        return result;
    }

    public AbstractModificator AddModificator(AbstractModificator modificator, AbstractModificator.ModificatorStatuses modificatorStatus)
    {
        for (int i = 0; i < CurrentModificators.Count; i++)
        {
            if (CurrentModificators[i].GetIsOverriding(modificator))
            {
                return null;
            }
            else if (CurrentModificators[i].GetIsRestrictedWith(modificator))
            {
                RemoveModificatorAt(i);
                i--;
            }
        }

        AbstractModificator newModificator = Instantiate(modificator, transform);
        newModificator.OriginalModificator = modificator;
        newModificator.Status = modificatorStatus;
        _currentModificators.Add(newModificator);

        ModificatorIcon newIcon = 
            UIManager.Instance.ModificatorsScreenOverlay.GetModificatorsUI()?.AddModificatorIcon(newModificator) ??
            UIManager.Instance.ArtifactModificatorsScreenOverlay.GetModificatorsUI()?.AddModificatorIcon(newModificator);

        if (!newModificator.DisabledModificator)
        {
            newModificator.OnModificatorAdded();
        }

        if (modificator.ModificatorType == AbstractModificator.ModificatorTypes.NEGATIVE)
        {
            SessionManager.Instance.CurrentSession.TotalObtainedCurses++;
        }

        _requestUpdateAvaibleModificators = true;
        return newModificator;
    }

    public void RemoveModificator(AbstractModificator modificator)
    {
        for (int i = 0; i < _currentModificators.Count; i++)
        {
            if (modificator == _currentModificators[i])
            {
                RemoveModificatorAt(i);
                break;
            }
        }
    }

    public void RemoveModificatorAt(int at)
    {
        UIManager.Instance?.ModificatorsScreenOverlay?.GetModificatorsUI()?.RemoveModificatorIcon(_currentModificators[at]);
        UIManager.Instance?.ArtifactModificatorsScreenOverlay?.GetModificatorsUI()?.RemoveModificatorIcon(_currentModificators[at]);
        Destroy(_currentModificators[at].gameObject);
        _currentModificators.RemoveAt(at);

        _requestUpdateAvaibleModificators = true;
    }

    public void RemoveModificators(AbstractModificator.ModificatorStatuses status)
    {
        while (true)
        {
            AbstractModificator removeModificator =
                CurrentModificators
                .Where(e => e.Status == status)
                .FirstOrDefault();

            if (removeModificator != null)
            {
                RemoveModificator(removeModificator);
            }
            else
            {
                break;
            }
        }
    }

    public List<AbstractModificator> PickRandomModificators(
        AbstractModificator.ModificatorTypes type,
        float minPrice,
        float maxPrice,
        bool allowPermanentIncapable = true,
        bool allowOverridePermanent = false,
        bool includeNeutral = true,
        List<AbstractModificator> excludeModificators = null,
        bool singleOnly = false
        )
    {
        List<AbstractModificator> result = new();
        IEnumerable<AbstractModificator> filteredModificators = AvaibleValidModificators.Where(e =>
            e.ModificatorType == type &&
            (allowPermanentIncapable || e.AllowPermanent) &&
            (excludeModificators == null || !excludeModificators.Contains(e)) &&
            !CurrentModificators.Any(e2 => e2.ModificatorPrice >= e.ModificatorPrice && e.GetIsOverriding(e2))
            );

        AbstractModificator singleModificatorResult =
            NumberMath.PickRandomItem(filteredModificators.Where(
                e => {
                    float overrideDependedPrice = e.GetPriceDependedOnOverrides(CurrentModificators);
                    return overrideDependedPrice >= minPrice && overrideDependedPrice <= maxPrice;
                }
            ).ToList());

        if (singleModificatorResult != null)
        {
            result.Add(singleModificatorResult);
        }
        else if (!singleOnly)
        {
            float totalModificatorsPrice = 0f;
            while (totalModificatorsPrice < minPrice && result.Count < MULTIPLE_MODIFICATORS_MAX_AMOUNT)
            {
                filteredModificators = filteredModificators.Where(e =>
                    result.All(e2 => e != e2 && !e.GetIsRestrictedWith(e2) && !e.GetIsOverriding(e2)) &&
                    e.ModificatorPrice <= maxPrice - totalModificatorsPrice
                    );

                if (filteredModificators.Count() == 0) break;
                AbstractModificator addModificator = filteredModificators.OrderBy(e => e.ModificatorPrice).Last();
                result.Add(addModificator);
                totalModificatorsPrice += addModificator.GetPriceDependedOnOverrides(CurrentModificators);
            }
        }

        if (result.Count > 0 && includeNeutral)
        {
            TryAddNeutralModificator(result, maxPrice);
        }

        return result;  
    }

    private void TryAddNeutralModificator(List<AbstractModificator> modificators, float maxPrice)
    {
        if (RandomManager.Instance.ProcRandomChance(ExtraNeutralModificatorChance, RandomManager.ProcChanceTypes.GOOD))
        {
            AbstractModificator neutralModificator = NumberMath.PickRandomItem(
                AvaibleValidModificators
                    .Where(e => e.ModificatorType == AbstractModificator.ModificatorTypes.NEUTRAL && e.ModificatorPrice < maxPrice)
                    .ToList()
                );

            if (neutralModificator != null)
            {
                modificators.Add(neutralModificator);
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
