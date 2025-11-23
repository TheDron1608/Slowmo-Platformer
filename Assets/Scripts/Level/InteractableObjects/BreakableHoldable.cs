using UnityEngine;

public class BreakableHoldable : BreakableObject
{
    [SerializeField] private int _maxUses = 10;
    public bool UnlimitedUses = true;

    private int _usesLeft;

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
                BreakObject(GetComponent<Holdable>()?.CurrentHolder);
            }
        }
    }

    private void Awake()
    {
        UsesLeft = MaxUses;
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
        {
            bool replacedBrokenHoldable = false;
            ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
            Vector2 spawnPosition = transform.position;
            if (TryGetComponent(out Collider2D collider))
            {
                spawnPosition += GameObjectUtility.GetCenterOfCollider(collider);
            }

            foreach (GameObject objectOnBreak in SpawnObjectsOnBreak)
            {
                if (!replacedBrokenHoldable && objectOnBreak.TryGetComponent(out Holdable holdableObjectOnBreak))
                {
                    GetComponent<Holdable>().TransformToAnotherObject(holdableObjectOnBreak);
                    replacedBrokenHoldable = true;
                }
                else
                {
                    GameObject newObjectOnBreak = Instantiate(objectOnBreak, transform);
                    layer.UpdateLayerForAllChildren(newObjectOnBreak.transform);
                    newObjectOnBreak.transform.position = spawnPosition;
                }
            }
        }

        SpawnBrokenParticles(breaker);

        RemoveAllStuckedObjects();

        Destroy(gameObject);
    }
}
