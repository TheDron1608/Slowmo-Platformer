using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractWithObjects : MonoBehaviour
{

    public float InteractRange = 1f;

    public List<InteractableObject> GetAvaibleInteractableObjects()
    {
        var result = new List<InteractableObject>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, InteractRange, InteractableObject.InteractableObjectsLayerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].gameObject.TryGetComponent(out InteractableObject interactableObjectComponent)) continue;

            result.Add(interactableObjectComponent);
        }

        return result;
    }

    public InteractableObject GetNearestAvaibleInteractableObject()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, InteractRange, InteractableObject.InteractableObjectsLayerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].gameObject.TryGetComponent(out InteractableObject interactableObjectComponent)) continue;

            return interactableObjectComponent;
        }
        return null;
    }

    public InteractableObject GetInteractableObjectAtDirection(Vector2 direction)
    {
        foreach (var raycastHit in Physics2D.RaycastAll(transform.position, direction, InteractRange, InteractableObject.InteractableObjectsLayerMask))
        {
            if (raycastHit.collider.TryGetComponent(out InteractableObject interactableObjectComponent)) return interactableObjectComponent;
        }
        return null;
    }
}
