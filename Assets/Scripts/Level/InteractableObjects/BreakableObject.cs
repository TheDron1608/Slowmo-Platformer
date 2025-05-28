using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakbleObject : MonoBehaviour
{
    [SerializeField] protected List<ParticleSpawner> _brokenPartsParticleSpawners;

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        Destroy(gameObject);

        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }
    }
}
