using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractWithObjects : MonoBehaviour
{

    public float InteractRange = 1f;

    public List<SelectableObject> GetAvaibleInteractableObjects()
    {
        var result = new List<SelectableObject>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, InteractRange);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].gameObject.TryGetComponent(out SelectableObject selectableObjectComponent)) continue;

            result.Add(selectableObjectComponent);
        }

        return result;
    }

    public SelectableObject GetInteractableObjectAtDirection(Vector2 direction)
    {
        foreach (var raycastHit in Physics2D.RaycastAll(transform.position, direction, InteractRange, 1 << gameObject.layer))
        {
            if (
                raycastHit.collider.TryGetComponent(out SelectableObject selectableObjectComponent) && 
                raycastHit.distance <= InteractRange * selectableObjectComponent.SelectMaxRangeMultiplier
                ) 
                return selectableObjectComponent;
        }
        return null;
    }
}
