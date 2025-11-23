using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteractWithObjects : AbstractCharacterComponent
{
    const float RAYCASTS_ACROSS_RADIAN_STEP = 0.05f;

    public event EventHandler<Interactable> OnInteracted;

    public float InteractRange = 1f;

    private bool _isAbleToInteractWithObjects = true;
    private Interactable _lastInteractObject = null;

    public bool IsAbleToInteractWithObjects
    {
        get => _isAbleToInteractWithObjects;
        set => _isAbleToInteractWithObjects = value;
    }

    public Interactable LastInteractObject
    {
        get => _lastInteractObject;
    }

    /// <summary>
    /// not includes holdables, use CharacterHolding.GetAvaibleHoldables() to get all them
    /// </summary>
    /// <returns></returns>
    public List<Interactable> GetAvaibleInteractables()
    {
        List<Interactable> result = new();

        result.AddRange(GetAvaibleInteractablesAtContainer(CharComponents.CharacterCollision.CurrentZLayer.FurnitureContainer));
        result.AddRange(GetAvaibleInteractablesAtContainer(CharComponents.CharacterCollision.CurrentZLayer.InteractableEnviromentContainer));

        return result;
    }

    private List<Interactable> GetAvaibleInteractablesAtContainer(Transform container)
    {
        List<Interactable> result = new();
        foreach (Transform furnitureTransform in container)
        {
            if (
                furnitureTransform.TryGetComponent(out Interactable interactableFurniture) &&
                interactableFurniture.GetIsValidToInteract(CharComponents.gameObject) &&
                Vector2.Distance(CharComponents.Center.transform.position, furnitureTransform.transform.position) <=
                    CharComponents.CharacterInteract.InteractRange * CharComponents.CharacterHolding.MaxGrabRangeMultiplier * interactableFurniture.SelectMaxRangeMultiplier
                )
            {
                result.Add(interactableFurniture);
            }
        }

        return result;
    }

    public bool TryInteract(Interactable interactable)
    {
        if (interactable.TryInteract(CharComponents.gameObject))
        {
            _lastInteractObject = interactable;
            OnInteracted?.Invoke(this, interactable);
            return true;
        }
        else
        {
            return false;
        }
    }
}
