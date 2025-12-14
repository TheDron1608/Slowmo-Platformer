using System;
using System.Collections.Generic;
using UnityEngine;

public class Debug_FixedModificatorStart : MonoBehaviour
{
    public List<ModificatorCardsCluster> CardsOnStart = new();
    [SerializeField] private ModificatorsContainer _container;


    private void Start()
    {
        foreach (var cluster in CardsOnStart)
        {
            _container.AddModificatorCardsCluster(Instantiate(cluster));
        }
    }
}