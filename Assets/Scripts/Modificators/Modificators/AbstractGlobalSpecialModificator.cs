using UnityEngine.InputSystem;

public abstract class AbstractGlobalSpecialModificator : AbstractModificator
{
    public InputActionReference SpecialActionReference;
    public int ComboCost = 0;

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
        if (
            !DisabledModificator &&
            !UIManager.GamePaused() && 
            SceneList.GetCurrentSceneIsGameplay() &&
            ScoreManager.Instance.CurrentCombo >= ComboCost
            )
        {
            if (OnSpecialActivated())
            {
                ScoreManager.Instance.CurrentCombo -= ComboCost;
            }
        }
    }

    public abstract bool OnSpecialActivated();
}