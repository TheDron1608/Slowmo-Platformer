using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ModificatorsManager : MonoBehaviour
{
    public static ModificatorsManager Instance;

    public List<AbstractModificator> AvaibleModificators = new();
    public int MaxModificatorOptions = 3;
    public int ModifiactorsPickAmount = 1;
    public float ExtraModificatorChance = 0.1f;
    public int MaxExtraModificators = 3;

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

    public List<ModificatorCardsCluster> PickRandomModifcators()
    {
        List<ModificatorCardsCluster> result = new(ModifiactorsPickAmount);
        for (int i = 0; i < MaxModificatorOptions; i++)
        {
            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            for (int j = 0; j < MaxExtraModificators; j++)
            {
                newCluster.AddCard(NumberMath.PickRandomItem(AvaibleModificators).CardInstance);
                if (Random.value > ExtraModificatorChance) break;
            }
            result.Insert(i, newCluster);
        }

        return result;  
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
