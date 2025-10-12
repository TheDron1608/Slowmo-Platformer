using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractParticle : MonoBehaviour
{
    public virtual void SetParticleAttrs(
        Vector2 position,
        Vector2 direction,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer,
        Sprite sprite = null,
        Animator animator = null,
        BoxCollider2D collider = null,
        string particleName = "untitled"
        )
    {
        gameObject.SetActive(true);
        transform.position = position;
        gameObject.name = particleName;
        if (sprite != null && material != null && TryGetComponent(out SpriteRenderer rendererComponent))
        {
            rendererComponent.sprite = sprite;
            rendererComponent.sharedMaterial = material;
        }
        if (animator != null && TryGetComponent(out Animator animatorComponent))
        {
            animatorComponent.runtimeAnimatorController = animator.runtimeAnimatorController;
        }
        if (collider != null && TryGetComponent(out BoxCollider2D colliderComponent))
        {
            colliderComponent.size = collider.size;
            colliderComponent.offset = collider.offset;
        }
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
