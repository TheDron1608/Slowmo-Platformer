using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputInteracting : AbstractAIInteracting
{
    const float ANGLE_STEP = 0.05f;

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
        if (UIManager.GamePaused()) return;
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

    private void FixedUpdate()
    {
        UpdateSelectedInteractObject();
    }

    private void UpdateSelectedInteractObject()
    {
        var interactables = CharComponents.CharacterInteract.GetAvaibleInteractables();

        if (interactables.Count > 0)
        {
            var interactableColliders = interactables.ConvertAll(e => e.GetComponent<Collider2D>());
            interactableColliders.Sort(
                (a, b) => a.bounds.SqrDistance(CharComponents.Center.transform.position).CompareTo(b.bounds.SqrDistance(CharComponents.Center.transform.position))
                );

            for (float angle = 0f; angle <= 1f; angle = angle > 0 ? -angle : -angle + ANGLE_STEP)
            {
                Ray ray = new(
                    CharComponents.Center.transform.position,
                    VectorMath.RotateVec2(CharComponents.CharacterAiming.GetTargetAimNormalized(), angle)
                    );

                //Debug.DrawRay(ray.origin, ray.direction, Color.red, Time.deltaTime);

                foreach (Collider2D interactableCollider in interactableColliders)
                {
                    if (interactableCollider.bounds.IntersectRay(ray))
                    {
                        CurrentSelectedInteractObject = interactableCollider.GetComponent<Interactable>();
                        return;
                    }
                }
            }
        }
        else
        {
            CurrentSelectedInteractObject = null;
        }
    }

    private void OnDestroy()
    {
        InteractActionReference.action.started -= InteractActionReference_OnActionStarted;
    }
}