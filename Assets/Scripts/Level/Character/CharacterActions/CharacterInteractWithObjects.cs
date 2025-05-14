using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterInteractWithObjects : AbstractCharacterComponent
{
    const float RAYCASTS_ACROSS_RADIAN_STEP = 0.05f;

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
    /// <typeparam name="T">
    /// sorts only colliders with type T, use MonoBehaviour if you dont need sorting
    /// </typeparam>
    /// <returns></returns>
    public List<T> GetAvaibleInteractableObjects<T>(int layerMask) where T : SelectableObject
    {
        if (!_isAbleToInteractWithObjects) return new List<T>();

        var result = new List<T>();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            CharComponents.Center.transform.position,
            InteractRange,
            layerMask
            );

        for (int i = 0; i < colliders.Length; i++)
        {
            if (
                colliders[i].gameObject.TryGetComponent(out SelectableObject selectableObjectComponent) &&
                Vector3.Distance(CharComponents.Center.transform.position, colliders[i].transform.position) <= selectableObjectComponent.SelectMaxRangeMultiplier * InteractRange &&
                selectableObjectComponent is T selectableObjectComponentSorted &&
                (!selectableObjectComponent.TryGetComponent(out Holdable holdable) || holdable.StuckedToCollider != CharComponents.CharacterRigidBodyCapsuleCollider)
                )
            {
                result.Add(selectableObjectComponentSorted);
            }
        }

        return result;
    }



    public T GetInteractableObjectAtEntireDirection<T>(Vector2 direction, int layerMask) where T : SelectableObject
    {
        for (float spread = 0f; spread < math.PI; spread += RAYCASTS_ACROSS_RADIAN_STEP)
        {
            T selectableObjectClockWise = GetInteractableObjectAtDirection<T>(VectorMath.RotateVec2(direction, spread), layerMask);
            if (selectableObjectClockWise != null) return selectableObjectClockWise;

            T selectableObjectCounterClockWise = GetInteractableObjectAtDirection<T>(VectorMath.RotateVec2(direction, -spread), layerMask);
            if (selectableObjectCounterClockWise != null) return selectableObjectCounterClockWise;
        }

        return null;
    }

    private T GetInteractableObjectAtDirection<T>(Vector2 direction, int layerMask) where T : SelectableObject
    {
        //Debug.DrawRay(CharComponents.Center.transform.position, direction);
        foreach (var raycastHit in Physics2D.RaycastAll(
                    CharComponents.Center.transform.position,
                    direction,
                    InteractRange,
                    layerMask
                    )
                )
        {
            if (
                raycastHit.collider.TryGetComponent(out SelectableObject selectableObjectComponent) &&
                raycastHit.distance <= InteractRange * selectableObjectComponent.SelectMaxRangeMultiplier &&
                selectableObjectComponent is T sortedSelectableObject &&
                (!selectableObjectComponent.TryGetComponent(out Holdable holdable) || holdable.StuckedToCollider != CharComponents.CharacterRigidBodyCapsuleCollider)
                )
            {
                return sortedSelectableObject;
            }
        }

        return null;
    }
}
