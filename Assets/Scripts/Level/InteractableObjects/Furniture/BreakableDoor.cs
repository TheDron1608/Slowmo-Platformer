using UnityEngine;

public class BreakableDoor : BreakableObject, IBreakableEntirelyObject
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

        SpawnBrokenParticlesAndPlaySound(breaker);

        GetComponent<SpriteRenderer>().flipX = breaker != null ? transform.position.x < breaker.transform.position.x : NumberMath.RandomCoinflip();
        GetComponent<OnInteractToggleOpenDoor>().IsOpen = true;
        GetComponent<OnInteractToggleOpenDoor>().enabled = false;
        if (TryGetComponent(out Animator anim))
        {
            anim.SetBool(ANIMATOR_DESTROYED_PROP_NAME, true);
        }
        if (TryGetComponent(out DamagableObject damagableObject))
        {
            damagableObject.HitableByMeleeProjectiles = false;
            damagableObject.HitableByRangedProjectiles = false;
        }
    }

    public void BreakObjectEntirely(MonoBehaviour breaker)
    {
        base.BreakObject(breaker);
    }
}
