using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputGrabbingAndThrowing : AbstractAIGrabbingAndThrowing
{
    const float ANGLE_STEP = 0.05f;

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
        if (UIManager.GamePaused()) return;
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
            CharComponents.CharacterHolding.TryGrab(CurrentSelectedGrabObject);
        }
        else
        {
            CharComponents.CharacterHolding.TryThrow(CharComponents.CharacterAiming.GetTargetAimNormalized());
        }
    }

    private void FixedUpdate()
    {
        UpdateSelectedGrabObject();
    }

    private void UpdateSelectedGrabObject()
    {
        if (!CharComponents.CharacterHolding.IsAbleToGrabObjects) return;

        var holdables = CharComponents.CharacterHolding.GetAvaibleHoldables();

        if (CharComponents.CharacterHolding.CurrentHoldObject == null && holdables.Count > 0)
        {
            CurrentSelectedGrabObject = holdables
                .OrderBy(HoldableOrderByPattern)
                .FirstOrDefault()
                ?.GetComponent<Holdable>();
        }
        else
        {
            CurrentSelectedGrabObject = null;
        }
    }

    private float HoldableOrderByPattern(Holdable go)
    {
        Collider2D collider = go?.GetComponent<Collider2D>();
        if (collider == null) return float.MaxValue;

        return
            (go.TryGetComponent(out Weapon weapon) && GetWeaponIsNotValidAsWeapon(weapon) ? 99999f : 0f) +
            collider.bounds.SqrDistance(CharComponents.Center.transform.position) *
            Vector2.Angle(
                collider.bounds.center - CharComponents.Center.transform.position, 
                CharComponents.CharacterAiming.GetTargetAimNormalized()
                );
    }

    private bool GetWeaponIsNotValidAsWeapon(Weapon weapon)
    {
        return
            (weapon.TryGetComponent(out RangedWeapon rangedWeapon) && rangedWeapon.GetIsOutOfAmmo()) ||
            (weapon.TryGetComponent(out Chainsaw chainsaw) && chainsaw.FuelLeft <= 0.05f) ||
            weapon.Tags.Contains(Weapon.WEAPON_TAGS.BROKEN);
    }

    private void OnDestroy()
    {
        GrabActionReference.action.started -= GrabActionReference_OnActionStarted;
    }
}