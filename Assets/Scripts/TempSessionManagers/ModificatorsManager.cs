using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ModificatorsManager : MonoBehaviour
{
    const int MULTIPLE_MODIFICATORS_ORDER_LIMIT = 10;
    const int MULTIPLE_MODIFICATORS_MAX_AMOUNT = 3;

    public static ModificatorsManager Instance;

    public List<AbstractModificator> AvaibleModificators = new();
    public int MaxModificatorOptions = 3;
    public int ModifiactorsPickAmount = 1;
    public float ExtraModificatorChance = 0.1f;
    public float ExtraNeutralModificatorChance = 0.1f;

    [SerializeField] private ModificatorCardsCluster _clusterInstance;

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

    public void AddModificator(AbstractModificator modificator)
    {
        AbstractModificator newModificator = Instantiate(modificator, transform);

        _currentModificators.Add(newModificator);
        if (UIManager.Instance?.ModificatorsScreenOverlay?.GetModificatorsUI() != null)
        {
            UIManager.Instance.ModificatorsScreenOverlay.GetModificatorsUI().AddModificatorIcon(modificator);
        }

        if (!newModificator.DisabledModificator)
        {
            newModificator.OnModificatorAdded();
        }
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

    public ModificatorCardsCluster PickRandomModifcator(AbstractModificator.ModificatorTypes type, float minPrice, float maxPrice)
    {
        ModificatorCardsCluster result = Instantiate(_clusterInstance);

        //try pick single modificator
        List<AbstractModificator> filteredModificators = 
            AvaibleModificators
                .Where(e => e.ModificatorType == type && e.ModificatorPrice >= minPrice && e.ModificatorPrice < maxPrice)
                .ToList();

        if (filteredModificators.Count > 0)
        {
            result.AddCard(NumberMath.PickRandomItem(filteredModificators).CardInstance);
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
                    result.AddCard(cheapModificator.CardInstance);
                    totalPrice += cheapModificator.ModificatorPrice;
                    addedAmount++;
                }
                else
                {
                    break;
                }
            }
        }

        if (RandomManager.Instance.ProcRandomChance(ExtraNeutralModificatorChance, RandomManager.ProcChanceTypes.GOOD))
        {
            AbstractModificator neutralModificator = NumberMath.PickRandomItem(
                AvaibleModificators
                    .Where(e => e.ModificatorType == AbstractModificator.ModificatorTypes.NEUTRAL && e.ModificatorPrice < maxPrice)
                    .ToList()
                );

            if (neutralModificator != null)
            {
                result.AddCard(neutralModificator.CardInstance);
            }
        }

        return result;  
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
