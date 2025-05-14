using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputGrabbingAndThrowing : AbstractAIGrabbingAndThrowing
{
    public InputActionReference GrabActionReference;
    public float GrabDangerousHoldablesExtraRange = 2.5f;

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
        if (CurrentSelectedGrabObject?.GetIsDangerouslyFast() ?? false)
        {
            CharComponents.CharacterInteract.InteractRange += GrabDangerousHoldablesExtraRange;
            CharComponents.CharacterHolding.TryGrab(CurrentSelectedGrabObject, true);
            CharComponents.CharacterInteract.InteractRange -= GrabDangerousHoldablesExtraRange;
        }
        else if (CharComponents.CharacterHolding.CurrentHoldObject == null)
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
        //trying catch dangerous thrown weapon with extra grab range
        CharComponents.CharacterInteract.InteractRange += GrabDangerousHoldablesExtraRange;

        Holdable avaibleToCatchDangerousHoldable =
            CharComponents.CharacterInteract.GetInteractableObjectAtEntireDirection<Holdable>(
                CharComponents.CharacterAiming.GetCurrentAimNormalized(),
                1 << CharComponents.CharacterCollision.CurrentZLayer.HoldablesLayer
            );

        CharComponents.CharacterInteract.InteractRange -= GrabDangerousHoldablesExtraRange;

        if (avaibleToCatchDangerousHoldable?.GetIsDangerouslyFast() ?? false)
        {
            CurrentSelectedGrabObject = avaibleToCatchDangerousHoldable;
        }

        //if there is no avaible to catch dangerous thrown weapons trying grab holdables regulary without extra range
        else if (CharComponents.CharacterHolding != null && CharComponents.CharacterHolding.CurrentHoldObject == null)
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