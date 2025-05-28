using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableDoor : BreakbleObject
{
    const string ANIMATOR_DESTROYED_PROP_NAME = "Destroyed";

    public override void BreakObject(MonoBehaviour breaker)
    {
        GetComponent<SpriteRenderer>().flipX = transform.position.x < breaker.transform.position.x;
        GetComponent<OnInteractToggleOpenDoor>().IsOpen = true;
        GetComponent<OnInteractToggleOpenDoor>().enabled = false;
        GetComponent<Animator>()?.SetBool(ANIMATOR_DESTROYED_PROP_NAME, true);

        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].transform.rotation = breaker.transform.rotation;
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }
    }
}
