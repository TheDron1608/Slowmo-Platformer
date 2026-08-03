using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterInteractWithObjects : AbstractCharacterComponent
{
    const float RAYCASTS_ACROSS_RADIAN_STEP = 0.05f;

    public event EventHandler<Interactable> OnInteracted;

    public float InteractRange = 1f;

    private bool _isAbleToInteractWithObjects = true;
    private Interactable _lastInteractObject = null;
    private AnimatedInteractable _currentAwaitingAnimatedInteractableFinish = null;

    public bool IsAbleToInteractWithObjects
    {
        get => _isAbleToInteractWithObjects;
        set => _isAbleToInteractWithObjects = value;
    }

    public Interactable LastInteractObject
    {
        get
        {
            if (_lastInteractObject != null && _lastInteractObject.IsDestroyed()) _lastInteractObject = null;
            return _lastInteractObject;
        }
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
        if (IsAbleToInteractWithObjects && interactable.TryInteract(CharComponents.gameObject))
        {
            _lastInteractObject = interactable;

            if (interactable.TryGetComponent(out AnimatedInteractable animatedInteractable))
            {
                if (_currentAwaitingAnimatedInteractableFinish != null)
                {
                    _currentAwaitingAnimatedInteractableFinish.OnFinishedInteract -= _currentAwaitingAnimatedInteractableFinish_OnFinishedInteract;
                }
                _currentAwaitingAnimatedInteractableFinish = animatedInteractable;
                _currentAwaitingAnimatedInteractableFinish.OnFinishedInteract += _currentAwaitingAnimatedInteractableFinish_OnFinishedInteract;
            }
            else
            {
                OnInteracted?.Invoke(this, interactable);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void _currentAwaitingAnimatedInteractableFinish_OnFinishedInteract(object sender, EventArgs e)
    {
        OnInteracted?.Invoke(this, _currentAwaitingAnimatedInteractableFinish);
        _currentAwaitingAnimatedInteractableFinish.OnFinishedInteract -= _currentAwaitingAnimatedInteractableFinish_OnFinishedInteract;
        _currentAwaitingAnimatedInteractableFinish = null;
    }

    private void OnDestroy()
    {
        if (_currentAwaitingAnimatedInteractableFinish != null)
        {
            _currentAwaitingAnimatedInteractableFinish.OnFinishedInteract -= _currentAwaitingAnimatedInteractableFinish_OnFinishedInteract;
        }
    }
}
