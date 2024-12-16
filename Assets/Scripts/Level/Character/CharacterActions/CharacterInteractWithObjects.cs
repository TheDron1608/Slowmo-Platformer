using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractWithObjects : MonoBehaviour
{

    public float InteractRange = 1f;

    public List<SelectableObject> GetAvaibleInteractableObjects()
    {
        var result = new List<SelectableObject>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, InteractRange, SelectableObject.InteractableObjectsLayerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].gameObject.TryGetComponent(out SelectableObject interactableObjectComponent)) continue;

            result.Add(interactableObjectComponent);
        }

        return result;
    }

    public SelectableObject GetInteractableObjectAtDirection(Vector2 direction)
    {
        foreach (var raycastHit in Physics2D.RaycastAll(transform.position, direction, InteractRange, SelectableObject.InteractableObjectsLayerMask))
        {
            if (
                raycastHit.collider.TryGetComponent(out SelectableObject interactableObjectComponent) && 
                raycastHit.distance <= InteractRange * interactableObjectComponent.SelectMaxRangeMultiplier
                ) 
                return interactableObjectComponent;
        }
        return null;
    }
}
