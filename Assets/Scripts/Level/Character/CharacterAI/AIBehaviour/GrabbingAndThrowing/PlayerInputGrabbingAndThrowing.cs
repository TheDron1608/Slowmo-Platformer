using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (GameplayUIManager.GamePaused()) return;
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
        CurrentSelectedGrabObject = CharComponents.CharacterHolding.GetAvaibleHoldables().OrderBy(
            (Holdable holdable) =>
                (VectorMath.Vec3ToVec2(holdable.transform.position - CharComponents.Center.transform.position).normalized - CharComponents.CharacterAiming.GetTargetAimNormalized()).magnitude +
                (!holdable.GetComponent<RangedWeapon>()?.GetIsOutOfAmmo() ?? true ? -1000f : 0f)
            ).FirstOrDefault();
    }

    private void OnDestroy()
    {
        GrabActionReference.action.started -= GrabActionReference_OnActionStarted;
    }
}