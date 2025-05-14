using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputGrabbingAndThrowing : AbstractAIGrabbingAndThrowing
{
    public InputActionReference GrabActionReference;

    private Holdable _currentSelectedGrabObject = null;

    public Holdable CurrentSelectedGrabObject
    {
        get => _currentSelectedGrabObject;
        private set
        {
            if (value != _currentSelectedGrabObject)
            {
                if (_currentSelectedGrabObject != null)
                {
                    _currentSelectedGrabObject.Selected = false;
                }
                if (value != null)
                {
                    value.Selected = true;
                }
            }
            _currentSelectedGrabObject = value;
        }
    }

    private void Start()
    {
        GrabActionReference.action.started += GrabActionReference_OnActionStarted;
    }

    private void GrabActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        HandleGrabThrow();
    }

    //GRAB
    private void HandleGrabThrow()
    {
        if (CharComponents.CharacterHolding.CurrentHoldObject == null)
        {
            if (CurrentSelectedGrabObject != null)
            {
                CharComponents.CharacterHolding.TryGrab(CurrentSelectedGrabObject);
            }
        }
        else
        {
            CharComponents.CharacterHolding.TryThrow(CharComponents.CharacterAiming.GetTargetAimNormalized());
        }
    }

    private void Update()
    {
        UpdateSelectedGrabObject();
    }

    private void UpdateSelectedGrabObject()
    {
        if (CharComponents.CharacterHolding != null && CharComponents.CharacterHolding.CurrentHoldObject == null)
        {
            CurrentSelectedGrabObject =
                CharComponents.CharacterInteract.GetInteractableObjectAtEntireDirection<Holdable>(
                    CharComponents.CharacterAiming.GetCurrentAimNormalized(),
                    1 << CharComponents.CharacterCollision.CurrentZLayer.HoldablesLayer
                );
        }
        else
        {
            CurrentSelectedGrabObject = null;
        }
    }

    private void OnDestroy()
    {
        GrabActionReference.action.started -= GrabActionReference_OnActionStarted;
    }
}