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
        var holdables = CharComponents.CharacterHolding.GetAvaibleHoldables();

        if (CharComponents.CharacterHolding.CurrentHoldObject == null && holdables.Count > 0)
        {
            var holdableColliders = holdables.ConvertAll(e => e.GetComponent<Collider2D>());
            holdableColliders.Sort(
                (a, b) => a.bounds.SqrDistance(CharComponents.Center.transform.position).CompareTo(b.bounds.SqrDistance(CharComponents.Center.transform.position))
                );

            for (float angle = 0f; angle <= 1f; angle = angle > 0 ? -angle : -angle + ANGLE_STEP)
            {
                Ray ray = new(
                    CharComponents.Center.transform.position,
                    VectorMath.RotateVec2(CharComponents.CharacterAiming.GetTargetAimNormalized(), angle)
                    );

                //Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);

                foreach (Collider2D holdableCollider in holdableColliders)
                {
                    if (holdableCollider.bounds.IntersectRay(ray))
                    {
                        CurrentSelectedGrabObject = holdableCollider.GetComponent<Holdable>();
                        return;
                    }
                }
            }
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