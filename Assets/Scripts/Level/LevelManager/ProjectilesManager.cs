using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectilesManager : MonoBehaviour
{
    [Header("Limits")]
    public int MeleeProjectilesMaxAmount = 128;
    public int RangedProjectilesMaxAmount = 128;
    [Header("UnusedInstancesContainers")]
    public Transform UnusedMeleeProjectilesContainer;
    public Transform UnusedRangedProjectilesContainer;
    [Header("SpawnInstances")]
    [SerializeField] private MeleeProjectile _emptyMeleeProjectileInstance;
    [SerializeField] private RangedProjectile _emptyRangedProjectileInstance;

    private List<MeleeProjectile> _meleeProjectiles;
    private List<RangedProjectile> _rangedProjectiles;

    public static ProjectilesManager Instance;

    private void Awake()
    {
        if (Instance != null) throw new Exception("limit of 1 ParticleManager per scene");
        Instance = this;

        InitParticles();
    }

    private void InitParticles()
    {
        _meleeProjectiles = new List<MeleeProjectile>(MeleeProjectilesMaxAmount);
        for (int i = 0; i < MeleeProjectilesMaxAmount; i++)
        {
            _meleeProjectiles.Insert(i, Instantiate(_emptyMeleeProjectileInstance, UnusedMeleeProjectilesContainer));
            _meleeProjectiles[i].gameObject.SetActive(false);
        }
        _rangedProjectiles = new List<RangedProjectile>(RangedProjectilesMaxAmount);
        for (int i = 0; i < RangedProjectilesMaxAmount; i++)
        {
            _rangedProjectiles.Insert(i, Instantiate(_emptyRangedProjectileInstance, UnusedRangedProjectilesContainer));
            _rangedProjectiles[i].gameObject.SetActive(false);
        }
    }

    public AbstractProjectile GetUnusedProjectile(AbstractProjectile prefab)
    {
        Transform unusedParticlesContainer = GetUnusedProjectilesContainerByType(prefab);
        if (unusedParticlesContainer.childCount == 0)
        {
            Instantiate(prefab, unusedParticlesContainer);
        }
        return unusedParticlesContainer.GetChild(0).GetComponent<AbstractProjectile>();
    }

    private Transform GetUnusedProjectilesContainerByType(AbstractProjectile prefab)
    {
        if (prefab is MeleeProjectile)
        {
            return UnusedMeleeProjectilesContainer;
        }
        else if (prefab is RangedProjectile)
        {
            return UnusedRangedProjectilesContainer;
        }
        else
        {
            throw new UnityException("could not find container for type " + (prefab?.name ?? "null"));
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
