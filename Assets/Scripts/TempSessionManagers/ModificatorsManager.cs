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

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 ModificatorsManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<ModificatorCardsCluster> PickRandomModifcators()
    {
        List<ModificatorCardsCluster> result = new(ModifiactorsPickAmount);
        for (int i = 0; i < MaxModificatorOptions; i++)
        {
            ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
            for (int j = 0; j < MaxExtraModificators; j++)
            {
                newCluster.AddCard(NumberMath.PickRandomItem(AvaibleModificators).Card);
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
