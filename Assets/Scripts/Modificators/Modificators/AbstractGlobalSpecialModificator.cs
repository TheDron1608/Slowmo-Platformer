using UnityEngine.InputSystem;

public abstract class AbstractGlobalSpecialModificator : AbstractModificator
{
    public InputActionReference SpecialActionReference;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        SpecialActionReference.action.started += SpecialActionRereference_OnActionStarted;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        SpecialActionReference.action.started -= SpecialActionRereference_OnActionStarted;
    }

    private void SpecialActionRereference_OnActionStarted(InputAction.CallbackContext context)
    {
        TryActivateSpecial();
    }

    public bool TryActivateSpecial()
    {
        if (
            !DisabledModificator &&
            !UIManager.GamePaused() &&
            SceneList.GetCurrentSceneIsGameplay() &&
            SpecialActivateCondition()
            )
        {
            return OnSpecialActivated();
        }
        else
        {
            return false;
        }
    }

    protected virtual bool SpecialActivateCondition()
    {
        return true;
    }

    public abstract bool OnSpecialActivated();

    private void OnDestroy()
    {
        SpecialActionReference.action.started -= SpecialActionRereference_OnActionStarted;
    }
}