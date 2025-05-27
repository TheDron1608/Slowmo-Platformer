using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakbleObject : MonoBehaviour
{
    [SerializeField] protected List<ParticleSpawner> _brokenPartsParticleSpawners;

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }

        Destroy(gameObject);
    }
}
