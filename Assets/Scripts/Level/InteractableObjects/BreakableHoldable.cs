using System.Linq;
using UnityEngine;

public class BreakableHoldable : BreakableObject
{
    [SerializeField] private int _maxUses = 10;
    [SerializeField] private int _usesLeft;
    public bool UnlimitedUses = true;


    public int MaxUses
    {
        get => _maxUses;
        set
        {
            _maxUses = value;
            if (_maxUses < _usesLeft)
            {
                _usesLeft = _maxUses;
            }
        }
    }

    public int UsesLeft
    {
        get => _usesLeft;
        set
        {
            _usesLeft = value;
            if (_usesLeft <= 0 && !UnlimitedUses)
            {
                if (TryGetComponent(out Holdable holdable))
                {
                    BreakObject(holdable.CurrentHolder);
                }
                else
                {
                    BreakObject(null);
                }
            }
        }
    }

    public void ResetUsesLeft()
    {
        UsesLeft = MaxUses;
    }

    public void SpendOneUse()
    {
        UsesLeft--;
    }

    public override void BreakObject(MonoBehaviour breaker)
    {
        Holdable replaceHoldable = null;
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        Vector2 spawnPosition = transform.position;
        if (TryGetComponent(out Collider2D collider))
        {
            spawnPosition += GameObjectUtility.GetCenterOfCollider(collider);
        }

        foreach (GameObject objectOnBreak in SpawnObjectsOnBreak)
        {
            if (replaceHoldable == null && objectOnBreak.TryGetComponent(out Holdable holdableObjectOnBreak))
            {
                replaceHoldable = holdableObjectOnBreak;
            }
            else
            {
                GameObject newObjectOnBreak = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TrySpawnObject(
                    objectOnBreak,
                    VectorMath.Vec3ToVec3Int(transform.position),
                    null,
                    null
                    )?.FirstOrDefault();

                if (newObjectOnBreak != null)
                {
                    newObjectOnBreak.transform.position = spawnPosition;
                }
            }
        }

        if (replaceHoldable != null)
        {
            BreakObjectVisualOnly(breaker);
            GetComponent<Holdable>().TransformToAnotherObject(replaceHoldable);
        }
        else
        {
            base.BreakObject(breaker);
        }
    }

    public void BreakObjectWithoutConvertToBroken(MonoBehaviour breaker)
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        Vector2 spawnPosition = transform.position;
        if (TryGetComponent(out Collider2D collider))
        {
            spawnPosition += GameObjectUtility.GetCenterOfCollider(collider);
        }

        foreach (GameObject objectOnBreak in SpawnObjectsOnBreak)
        {
            if (objectOnBreak.TryGetComponent(out Holdable h)) continue;

            GameObject newObjectOnBreak = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TrySpawnObject(
                objectOnBreak,
                VectorMath.Vec3ToVec3Int(transform.position),
                null,
                null
                )?.FirstOrDefault();

            if (newObjectOnBreak != null)
            {
                newObjectOnBreak.transform.position = spawnPosition;
            }
        }

        base.BreakObject(breaker);
    }
}
