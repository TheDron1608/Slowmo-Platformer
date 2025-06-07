using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public List<GameObject> SpawnObjectsOnBreak = new();

    [SerializeField] protected List<ParticleSpawner> _brokenPartsParticleSpawners;

    public virtual void BreakObject(MonoBehaviour breaker)
    {
        {
            ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
            Vector2 spawnPosition;
            if (TryGetComponent(out Collider2D collider))
            {
                spawnPosition = GameObjectUtility.GetCenterOfCollider(collider);
            }
            else
            {
                spawnPosition = transform.position;
            }

            Vector3 position = GameObjectUtility.GetCenterOfCollider(GetComponent<Collider2D>());
            foreach (GameObject objectInside in SpawnObjectsOnBreak)
            {
                GameObject newObject = Instantiate(objectInside, transform);
                LayerManager.Instance.ChangeZIndexForGameObject(layer, newObject);
                newObject.transform.position = position;
            }
            SpawnObjectsOnBreak.Clear();
        }

        for (int i = 0; i < _brokenPartsParticleSpawners.Count; i++)
        {
            _brokenPartsParticleSpawners[i].SpawnParticle();
        }

        Destroy(gameObject);
    }
}
