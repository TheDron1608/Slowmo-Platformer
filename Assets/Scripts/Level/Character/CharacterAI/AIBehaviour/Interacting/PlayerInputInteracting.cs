using System;
using System.Collections;
using System.Linq;
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
        if (GameplayUIManager.GamePaused()) return;
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
        CurrentSelectedInteractObject = CharComponents.CharacterInteract.GetAvaibleInteractables().Where(
            (Interactable interactable) => interactable.enabled && interactable.gameObject.activeSelf
            ).OrderBy(
            (Interactable interactable) =>
                (VectorMath.Vec3ToVec2(interactable.transform.position - CharComponents.Center.transform.position).normalized - CharComponents.CharacterAiming.GetTargetAimNormalized()).magnitude * 
                Vector2.Distance(CharComponents.Center.transform.position, interactable.transform.position)
            ).FirstOrDefault();
    }

    private void OnDestroy()
    {
        InteractActionReference.action.started -= InteractActionReference_OnActionStarted;
    }
}