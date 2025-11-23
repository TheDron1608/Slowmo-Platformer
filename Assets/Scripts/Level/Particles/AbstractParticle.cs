using UnityEngine;

public abstract class AbstractParticle : MonoBehaviour
{
    private bool _isSpawned = false;
    public bool IsSpawned
    {
        get => _isSpawned;
        private set => _isSpawned = value;
    }

    public virtual void SetParticleAttrs(
        AbstractParticle original,
        Vector2 position,
        Vector2 direction,
        float angle,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer
        )
    {
        IsSpawned = true;
        gameObject.SetActive(true);
        transform.position = VectorMath.Vec2ToVec3(position, transform.position.z);
        gameObject.name = original.gameObject.name;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new(0, 0, angle);
        transform.rotation = newRotation;

        LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);
        transform.SetAsLastSibling();
    }

    public virtual void RemoveParticle()
    {
        IsSpawned = false;
        gameObject.SetActive(false);
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
    }
}
