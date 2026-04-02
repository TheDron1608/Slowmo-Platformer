using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ModificatorsManager : MonoBehaviour
{
    const int MULTIPLE_MODIFICATORS_ORDER_LIMIT = 10;
    const int MULTIPLE_MODIFICATORS_MAX_AMOUNT = 3;
    const float MIN_SINGLE_MODIFICATOR_REQUIRED_PRICE = 0.75f;

    public static ModificatorsManager Instance;

    public List<AbstractModificator> AvaibleModificators = new();
    public int MaxModificatorOptions = 3;
    public int ModifiactorsPickAmount = 1;
    public float ExtraModificatorChance = 0.1f;
    public float ExtraNeutralModificatorChance = 0.1f;

    [Header("Instances")]
    [SerializeField] private ModificatorCardsCluster _clusterInstance;
    [SerializeField] private ModificatorIcon _emptyIconIstance;
    [SerializeField] private ModificatorCard _emptyCardIstance;

    [SerializeField] private List<Sprite> _characterIconBg;
    [SerializeField] private List<Sprite> _permanentIconBg;
    [SerializeField] private List<Sprite> _tradableIconBg;

    [SerializeField] private List<Material> _positiveCardTierMaterials = new();
    [SerializeField] private List<Material> _negativeCardTierMaterials = new();
    [SerializeField] private List<Material> _neutralCardTierMaterials = new();

    private List<AbstractModificator> _currentModificators = new();

    public List<AbstractModificator> CurrentModificators
    {
        get => _currentModificators;
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
        result.TitleImage.sprite = modificator.IconSprite;
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
        }

        return result;
    }

    public ModificatorCard CreateModificatorCard(AbstractModificator modificator, Transform parent)
    {
        ModificatorCard result = Instantiate(_emptyCardIstance, parent);
        result.name = modificator.name + "Card";
        result.ModificatorInstance = modificator;
        result.TitleImage.sprite = modificator.CardSprite;
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
        result.Localization = Instantiate(modificator.Localization, result.transform);

        return result;
    }

    public AbstractModificator AddModificator(AbstractModificator modificator, AbstractModificator.ModificatorStatuses modificatorStatus)
    {
        AbstractModificator newModificator = Instantiate(modificator, transform);
        newModificator.OriginalModificator = modificator;
        newModificator.Status = modificatorStatus;
        _currentModificators.Add(newModificator);
        if (UIManager.Instance?.ModificatorsScreenOverlay?.GetModificatorsUI() != null)
        {
            UIManager.Instance.ModificatorsScreenOverlay.GetModificatorsUI().AddModificatorIcon(newModificator);
        }

        if (!newModificator.DisabledModificator)
        {
            newModificator.OnModificatorAdded();
        }

        return newModificator;
    }

    public void RemoveModificator(AbstractModificator modificator)
    {
        for (int i = 0; i < _currentModificators.Count; i++)
        {
            if (modificator.GetEqualType(_currentModificators[i]))
            {
                if (UIManager.Instance?.ModificatorsScreenOverlay != null)
                {
                    UIManager.Instance.ModificatorsScreenOverlay.GetModificatorsUI().RemoveModificatorIcon(_currentModificators[i]);
                }
                Destroy(_currentModificators[i].gameObject);
                _currentModificators.RemoveAt(i);

                break;
            }
        }
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

    public ModificatorCardsCluster PickRandomModificators(AbstractModificator.ModificatorTypes type, float price)
    {
        ModificatorCardsCluster result = Instantiate(_clusterInstance);

        List<AbstractModificator> filteredModificators = 
            AvaibleModificators
            .Where(e => e.ModificatorType == type && e.ModificatorPrice <= price)
            .ToList();

        float totalPrice = 0;
        List<AbstractModificator> addedModificators = new();
        while (totalPrice < price * MIN_SINGLE_MODIFICATOR_REQUIRED_PRICE && addedModificators.Count < MULTIPLE_MODIFICATORS_MAX_AMOUNT)
        {
            AbstractModificator newModificator = 
                filteredModificators
                .Where(e => e.ModificatorPrice < price - totalPrice)
                .Where(e => !addedModificators.Contains(e))
                .OrderBy(e => math.abs(e.ModificatorPrice - price))
                .FirstOrDefault();

            addedModificators.Add(newModificator);
            totalPrice += newModificator.ModificatorPrice;

            result.AddModificator(newModificator);
        }

        if (result.Cards.Count == 0) return null;

        TryAddNeutralModificatorToCluster(result, price);

        return result;
    }

    public ModificatorCardsCluster PickRandomModificators(AbstractModificator.ModificatorTypes type, float minPrice, float maxPrice)
    {
        ModificatorCardsCluster result = Instantiate(_clusterInstance);

        //try pick single modificator
        List<AbstractModificator> filteredModificators = 
            AvaibleModificators
            .Where(e => e.ModificatorType == type && e.ModificatorPrice >= minPrice && e.ModificatorPrice < maxPrice)
            .ToList();

        if (filteredModificators.Count > 0)
        {
            result.AddModificator(NumberMath.PickRandomItem(filteredModificators));
        }
        //if failed pick single modificaotr pick multiple cheap modificators
        else
        {
            float totalPrice = 0;
            int addedAmount = 0;
            while (totalPrice < minPrice && addedAmount < MULTIPLE_MODIFICATORS_MAX_AMOUNT)
            {
                AbstractModificator cheapModificator = NumberMath.PickRandomItem(
                    AvaibleModificators
                        .Where(e => e.ModificatorType == type && e.ModificatorPrice < maxPrice - totalPrice)
                        .OrderByDescending(e => e.ModificatorPrice)
                        .Take(MULTIPLE_MODIFICATORS_ORDER_LIMIT)
                        .ToList()
                    );

                if (cheapModificator != null)
                {
                    result.AddModificator(cheapModificator);
                    totalPrice += cheapModificator.ModificatorPrice;
                    addedAmount++;
                }
                else
                {
                    break;
                }
            }
        }

        if (result.Cards.Count == 0) return null;

        TryAddNeutralModificatorToCluster(result, maxPrice);

        return result;  
    }

    private void TryAddNeutralModificatorToCluster(ModificatorCardsCluster cluster, float maxPrice)
    {
        if (RandomManager.Instance.ProcRandomChance(ExtraNeutralModificatorChance, RandomManager.ProcChanceTypes.GOOD))
        {
            AbstractModificator neutralModificator = NumberMath.PickRandomItem(
                AvaibleModificators
                    .Where(e => e.ModificatorType == AbstractModificator.ModificatorTypes.NEUTRAL && e.ModificatorPrice < maxPrice)
                    .ToList()
                );

            if (neutralModificator != null)
            {
                cluster.AddModificator(neutralModificator);
            }
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
