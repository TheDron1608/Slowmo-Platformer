using UnityEngine;

public class BreakableDoor : BreakableObject
{
    const string ANIMATOR_DESTROYED_PROP_NAME = "Destroyed";

    public override void BreakObject(MonoBehaviour breaker)
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

            foreach (GameObject objectOnBreak in SpawnObjectsOnBreak)
            {
                GameObject newObjectOnBreak = Instantiate(objectOnBreak, transform);
                layer.UpdateLayerForAllChildren(newObjectOnBreak.transform);
                newObjectOnBreak.transform.position = spawnPosition;
            }

            RemoveAllStuckedObjects();

            SpawnObjectsOnBreak.Clear();
        }

        SpawnBrokenParticles(breaker);

        GetComponent<SpriteRenderer>().flipX = transform.position.x < breaker.transform.position.x;
        GetComponent<OnInteractToggleOpenDoor>().IsOpen = true;
        GetComponent<OnInteractToggleOpenDoor>().enabled = false;
        GetComponent<Animator>()?.SetBool(ANIMATOR_DESTROYED_PROP_NAME, true);
        if (TryGetComponent(out DamagableObject damagableObject))
        {
            damagableObject.HitableByMeleeProjectiles = false;
            damagableObject.HitableByRangedProjectiles = false;
        }
    }
}
