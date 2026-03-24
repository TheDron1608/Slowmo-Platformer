using System;
using System.Collections.Generic;
using UnityEngine;

public class Debug_FixedModificatorStart : MonoBehaviour
{
    public List<ModificatorCardsCluster> CardsOnStart = new();
    public AbstractModificator.ModificatorStatuses CardsStatus;
    [SerializeField] private AbstractModificatorCardsManager _container;


    private void Start()
    {
        foreach (var cluster in CardsOnStart)
        {
            ModificatorCardsCluster newCluster = Instantiate(cluster);
            newCluster.AddStatusOnPick = CardsStatus;
            _container.AddModificatorCardsCluster(newCluster);
        }
    }
}