using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputInteracting : AbstractAIInteracting
{
    public InputActionReference InteractActionReference;

    private Interactable _currentSelectedInteractObject = null;

    public Interactable CurrentSelectedInteractObject
    {
        get => _currentSelectedInteractObject;
        private set
        {
            if (value != _currentSelectedInteractObject)
            {
                if (_currentSelectedInteractObject != null)
                {
                    _currentSelectedInteractObject.Selected = false;
                }
                if (value != null)
                {
                    value.Selected = true;
                }
            }
            _currentSelectedInteractObject = value;
        }
    }

    private void Start()
    {
        InteractActionReference.action.started += InteractActionReference_OnActionStarted;
    }

    private void InteractActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleInteract();
    }

    //INTERACT
    private void HandleInteract()
    {
        if (CurrentSelectedInteractObject != null)
        {
            CharComponents.CharacterInteract.TryInteract(CurrentSelectedInteractObject);
        }
    }

    private void Update()
    {
        UpdateSelectedInteractObject();
    }

    private void UpdateSelectedInteractObject()
    {
        if (CharComponents.CharacterInteract != null)
        {
            CurrentSelectedInteractObject =
                CharComponents.CharacterInteract.GetInteractableObjectAtEntireDirection<Interactable>(
                    CharComponents.CharacterAiming.GetCurrentAimNormalized(),
                    CharComponents.CharacterCollision.CurrentZLayer.EntireLayerMask - (1 << CharComponents.CharacterCollision.CurrentZLayer.HoldablesLayer)
                );
        }
    }

    private void OnDestroy()
    {
        InteractActionReference.action.started -= InteractActionReference_OnActionStarted;
    }
}