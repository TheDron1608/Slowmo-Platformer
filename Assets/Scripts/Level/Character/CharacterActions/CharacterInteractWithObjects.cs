using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractWithObjects : AbstractCharacterComponent
{

    public float InteractRange = 1f;

    private bool _isAbleToInteractWithObjects = true;

    public bool IsAbleToInteractWithObjects
    {
        get => _isAbleToInteractWithObjects;
        set => _isAbleToInteractWithObjects = value;
    }

    /// <summary>
    /// Returns all objects in a circle with randius of InteractRange property
    /// </summary>
    /// <param name="sortInteractType">
    /// If not null, returns a list of Selectable objects with equal InteractType property to this argument,
    /// else returns all Selectable objects
    /// </param>
    /// <returns></returns>
    public List<SelectableObject> GetAvaibleInteractableObjects(SelectableObject.SelectableObjectType? sortInteractType = null)
    {
        if (!_isAbleToInteractWithObjects) return new List<SelectableObject>();

        var result = new List<SelectableObject>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            CharComponents.Center.transform.position, 
            InteractRange
            );

        for (int i = 0; i < colliders.Length; i++)
        {
            if (
                !colliders[i].gameObject.TryGetComponent(out SelectableObject selectableObjectComponent) ||
                Vector3.Distance(CharComponents.Center.transform.position, colliders[i].transform.position) > selectableObjectComponent.SelectMaxRangeMultiplier  * InteractRange
                ) continue;

            if (sortInteractType is null || sortInteractType.Value == selectableObjectComponent.ObjectType)
            result.Add(selectableObjectComponent);
        }

        return result;
    }

    public SelectableObject GetInteractableObjectAtDirection(Vector2 direction)
    {
        if (!_isAbleToInteractWithObjects) return null;

        foreach (var raycastHit in Physics2D.RaycastAll(
                CharComponents.Center.transform.position, 
                direction, 
                InteractRange
                )
            )
        {
            if (
                raycastHit.collider.TryGetComponent(out SelectableObject selectableObjectComponent) &&
                raycastHit.distance <= InteractRange * selectableObjectComponent.SelectMaxRangeMultiplier
                )
            {
                return selectableObjectComponent;
            }
        }
        return null;
    }
}
