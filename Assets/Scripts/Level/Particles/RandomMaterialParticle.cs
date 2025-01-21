using System.Collections.Generic;
using UnityEngine;

public class RandomMaterialParticle : MonoBehaviour
{
    public List<Material> Materials = new();

    private void Awake()
    {
        GetComponent<ParticleSystemRenderer>().material = Materials[(int)Mathf.Floor(Random.value * Materials.Count)];
    }
}
