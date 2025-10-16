using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractParticle : MonoBehaviour
{
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
        gameObject.SetActive(true);
        transform.position = VectorMath.Vec2ToVec3(position, transform.position.z);
        gameObject.name = original.gameObject.name;

        Quaternion newRotation = new();
        newRotation.eulerAngles = new(0, 0, angle);
        transform.rotation = newRotation;

        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        SpriteRenderer originalRenderer = original.GetComponent<SpriteRenderer>();
        renderer.sprite = originalRenderer.sprite;
        renderer.sharedMaterial = material ?? originalRenderer.sharedMaterial;

        LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);
    }

    public virtual void RemoveParticle()
    {
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
